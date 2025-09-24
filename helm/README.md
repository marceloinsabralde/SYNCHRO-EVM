This directory contains Helm templates and BCSM configuration for deploying EVM to Azure Kubernetes.

Here's some notes on spinning this up for local testing and connecting to an Azure Kubernetes cluster for debugging.

# Running Kubernetes locally

The easiest way to create a local Kubernetes cluster locally seems to be using `kind` (Kubernetes in Docker) via Podman Desktop.
Something like `k3d` or `minikube` should work fine too if you're familiar with them but these have not been tested and scripts in this repo probably won't support them.

## Provisioning

Reference: https://kind.sigs.k8s.io/docs/user/ingress/

Install required CLI tools:

```
brew install kubernetes-cli kind helm
```

Create a kind cluster with mapped ports for ingress:

```
cat <<EOF | KIND_EXPERIMENTAL_PROVIDER=podman kind create cluster --config=-
kind: Cluster
apiVersion: kind.x-k8s.io/v1alpha4
nodes:
- role: control-plane
  extraPortMappings:
  - containerPort: 80
    hostPort: 9090
    protocol: TCP
  - containerPort: 443
    hostPort: 9443
    protocol: TCP
EOF
```

Install the nginx ingress controller:

```
helm upgrade --install \
  --create-namespace --namespace ingress-nginx \
  ingress-nginx ingress-nginx \
  --repo https://kubernetes.github.io/ingress-nginx \
  --version 4.0.17 \
  --set controller.hostPort.enabled=true \
  --set controller.service.type=NodePort \
  --wait
```

### GlobalProtect

GlobalProtect is now injecting a `online.local` DNS search zone which results in DNS lookups being slow enough that connections often fail.
Here's a script to drop all DNS search zones which GlobalProject has injected. You'll need to do this every time GlobalProtect refreshes the connection. If things were working and they've stopped, try this first.

Run `sudo script/delete-global-protect-search-domains` to delete any DNS search domains.
If any diff was outputted then continue on to apply the changes to Podman.
Run `podman machine stop && podman machine start` to restart Podman Machine.
Run `podman start kind-control-plane` to restart the Kubernetes cluster.
Run `script/dcl up --wait` to restart local containers

## Deploying

Run `script/docker-build` to build a container image.
Run `script/kind-load` to load that container image into to the cluster.
Run `script/helm-install Kumara.WebApi` to provision Web API into the cluster.

## Testing the service

Run `kubectl port-forward service/kumara-web-api 50000:80` to temporarily forward TCP 50000 to the service.
Run `curl http://localhost:50000/healthz` to confirm it's working. It should reply with “Healthy”.
Stop the `kubectl port-forward` with Ctrl-C

## Testing ingress

Run `curl -i http://localhost:9090/healthz -H Host:kumara-web-api.localhost`
This should also respond with "Healthy" confirming ingress is wired up.

## Accessing ingress with your browser

If you want to have your normal HTTPS dev URL point to ingress you can run `script/helm-install --dev-cert Kumara.WebApi`.
This will wire up ingress as "localhost" and configure HTTPS with the .NET Core localhost dev certificate.
The script will output a `socat` command you can run to temporarily forward the correct TCP port to ingress.
You should then be able to succesfully load https://localhost:7029/swagger/ in your browser.

## Cleanup

Kubernetes clusters use a fair bit of CPU even when idle so you you probably don't want to keep it around unless you're using it.
You can uninstall the app from the cluster with `helm uninstall kumara-web-api`.
You can delete the Kubernetes cluster from your Podman with `kind delete cluster`.

# Accessing the BCSM DEV cluster

A few of us have access to the "BentleyConnect Containers Dev" subscription and can access our DEV cluster directly for debugging.

## Enable access to the cluster

Navigate to [dev-SynchroAKS-eus](https://portal.azure.com/#@bentley.com/resource/subscriptions/84a8d6cf-0ec6-41dd-a343-932c199ae46b/resourceGroups/dev-SynchroAKS-eus-rg/providers/Microsoft.ContainerService/managedClusters/dev-synchroaks-eus) in Azure Portal.
Under Settings > Security configuration > Cluster admin ClusterRoleBinding, ensure “BCSM - Synchro Perform - contributors” is in the list. If not, edit and add `a85b69cb-1d5f-4dd6-801e-9c80561472f7`. Wait for the changes to save.
Under Settings > Networking > Authorized IP ranges, add your outbound IPv4 address to the existing list. Use `curl -4 ifconfig.me` to find your current outbound IPv4 address. Wait for the changes to save.
Note: these settings will be overriden whenever the [SYNCHRO Kubernetes Cluster (SKCA)](https://dev.azure.com/bentleycs/beconnect/_release?definitionId=1951) pipeline is ran, which is generally pretty rare.

## Configure local client to use the cluster

Run `brew install kubelogin` to install the authentication helper.
Run `az aks get-credentials --subscription "BentleyConnect Containers Dev" --resource-group "dev-SynchroAKS-eus-rg" --name "dev-synchroaks-eus"` to setup a context in your Kubernetes client.

Tip: To use Podman Desktop to inspect/control the Azure Kubernetes cluster, you'll need to start it with `open -a "Podman Desktop"`. This passes through the shell's `PATH` so Podman Desktop can find the authentication helper.

## Accessing services without ingress hosted on the cluster

Run `kubectl port-forward -n synchro service/evm-core 50000:80` to temporarily forward TCP 50000 to the service.

## Cleanup

Once you're done, either clear your context selection with `kubectl config unset current-context` or
switch back to your local context with `kubectl config use-context kind-kind-cluster`.
This will ensure you don't accidently modify the BCSM DEV context later on.
