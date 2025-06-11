// Copyright (c) Bentley Systems, Incorporated. All rights reserved.

using System.Collections;
using System.Xml;
using System.Xml.Linq;

class StringResourceDictionary
{
    private readonly XNamespace xml = "http://www.w3.org/XML/1998/namespace";
    private readonly XNamespace wpf = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private readonly XNamespace x = "http://schemas.microsoft.com/winfx/2006/xaml";
    private readonly XNamespace s = "clr-namespace:System;assembly=mscorlib";

    private XDocument document;

    public StringResourceDictionary()
    {
        document = new XDocument(
            new XElement(
                wpf + "ResourceDictionary",
                new XAttribute(XNamespace.Xmlns + "wpf", wpf),
                new XAttribute(XNamespace.Xmlns + "x", x),
                new XAttribute(XNamespace.Xmlns + "s", s)
            )
        );
    }

    public void Load(string path)
    {
        document = XDocument.Load(path);

        NormalizeWhitespace();
    }

    public void Save(string path)
    {
        using (var stream = new StreamWriter(path))
        {
            var settings = new XmlWriterSettings
            {
                OmitXmlDeclaration = true,
                Indent = true,
                IndentChars = "\t",
            };
            using (var writer = XmlWriter.Create(stream, settings))
            {
                document.Save(writer);
            }
            stream.WriteLine();
        }
    }

    public string? this[string key]
    {
        get
        {
            var element = GetStringElement(key);
            return element?.Value;
        }
        set
        {
            var element = GetStringElement(key) ?? AddStringElement(key);
            if (value is not null)
            {
                element.Value = value;
            }
            else
            {
                element.Remove();
            }
        }
    }

    private void NormalizeWhitespace()
    {
        document.Root?.Attribute(xml + "space")?.Remove();
        document.Root?.DescendantNodes()?.OfType<XText>()?.Remove();
    }

    private XElement? GetStringElement(string key)
    {
        return document
            .Root!.Elements(s + "String")
            .FirstOrDefault(e => e.Attribute(x + "Key")?.Value == key);
    }

    private XElement AddStringElement(string key, string? value = null)
    {
        var element = new XElement(s + "String", new XAttribute(x + "Key", key), value);
        document.Root!.Add(element);
        return element;
    }
}
