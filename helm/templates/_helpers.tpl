{{- define "name" -}}
{{- default .Release.Name .Values.nameOverride | trunc 63 | trimSuffix "-" }}
{{- end }}

{{- define "selectorLabels" -}}
app.kubernetes.io/name: {{ include "name" . }}
app.kubernetes.io/instance: {{ include "name" . }}
{{- end }}

{{- define "labels" -}}
helm.sh/chart: "{{ .Chart.Name }}-{{ .Chart.Version }}"
{{ include "selectorLabels" . }}
app.kubernetes.io/managed-by: {{ .Release.Service }}
app.kubernetes.io/part-of: {{ (split "." .Values.project.name)._0 | lower }}
app.kubernetes.io/component: {{ regexReplaceAll "([a-z0-9])([A-Z])" (split "." .Values.project.name)._1 "${1}-${2}" | lower }}
app.kubernetes.io/version: {{ .Values.image.tag }}
{{- end }}

{{- define "metadata" -}}
name: {{ include "name" . }}
namespace: {{ .Release.Namespace }}
labels:
  {{- include "labels" . | nindent 2 }}
{{- end }}
