using System;
using System.IO;
using System.Xml;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RDS.TextParser.Tokens;

namespace RDS.TextParser.Types
{
    public class SourceConfig
    {
        public Dictionary<string, Token> Tokens = new Dictionary<string, Token>();

        private string[] configPaths;

        public string[] ConfigPaths
        {
            get => configPaths;
            set
            {
                configPaths = value ?? Array.Empty<string>();
                changedConfigPaths = true;
            }
        }

        private string CurrentConfig;
        private DateTime timestamp;
        private bool changedConfigPaths = true;
        private XmlDocument doc;


        public bool LoadConfigFromString(string configXML)
        {
            Tokens.Clear();

            bool result;

            try
            {
                if (!LoadConfigString(configXML))
                {
                    throw new Exception("Could not load provided configuration.");
                }

                var tokens = doc.SelectNodes("//Token");

                foreach (XmlNode node in tokens)
                {
                    ParseToken(node, ref Tokens);
                }

                result = true;
            }
            catch (Exception err)
            {
                throw err;
            }

            return result;
        }

        private bool LoadConfigString(string config)
        {
            if ((config == null) || (config.Length == 0)) { return false; }

            try
            {
                doc = new XmlDocument();
                doc.LoadXml(config);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }

            return true;
        }

        private void ParseToken(XmlNode node, ref Dictionary<string, Token> tokens)
        {
            string kind = (node.Attributes["kind"] == null) ? string.Empty : node.Attributes["kind"].Value.Trim();
            if (kind.Length == 0)
            {
                kind = "Text";
            }

            string attName = (node.Attributes["name"] == null) ? kind : node.Attributes["name"].Value;

            if (attName.Length == 0)
            {
                attName = string.Concat("Token_", tokens.Count.ToString());
            }

            switch (kind)
            {
                case ("Text"):
                    if (!tokens.ContainsKey(attName))
                    {
                        Token token = CreateTextToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("Type"):
                    if (!tokens.ContainsKey(attName))
                    {
                        Type_Token token = CreateTypeToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("Conditional"):
                    if (!tokens.ContainsKey(attName))
                    {
                        ConditionalToken token = CreateConditionalToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("List"):
                    if (!tokens.ContainsKey(attName))
                    {
                        ListToken token = CreateListToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("TypeList"):
                    if (!tokens.ContainsKey(attName))
                    {
                        TypeListToken token = CreateTypeListToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("Table"):
                    if (!tokens.ContainsKey(attName))
                    {
                        TypeListToken token = CreateTableToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                case ("JSON"):
                    if (!tokens.ContainsKey(attName))
                    {
                        JSONToken token = CreateJSONToken(node);
                        if (token != null) { tokens.Add(attName, token); }
                    }
                    break;
                default:

                    break;
            }
        }

        private JSONToken CreateJSONToken(XmlNode node)
        {
            JSONToken token = new JSONToken(CreateTextToken(node));

            XmlNode attNode = node.Attributes["key"];
            token.Key = (attNode == null) ? string.Empty : attNode.Value;

            return token;
        }

        private TypeListToken CreateTypeListToken(XmlNode node)
        {
            ListToken lToken = CreateListToken(node);

            if (lToken == null) { return null; }

            TypeListToken token = new TypeListToken(lToken);
            node = node.SelectSingleNode("Type");

            token.TypeToken = CreateTypeToken(node);

            return token;
        }

        private TableToken CreateTableToken(XmlNode node)
        {
            ListToken lToken = CreateListToken(node, "Table", "Row");

            if (lToken == null) { return null; }

            TableToken token = new TableToken(lToken);

            node = node.SelectSingleNode("Columns");

            token.TypeToken = CreateTypeToken(node);

            return token;
        }

        private ListToken CreateListToken(XmlNode node, string listNodeName = "List", string elementNodeName = "Element")
        {
            XmlNode listNode = node.SelectSingleNode(listNodeName);
            if (listNode == null) { return null; }

            if (!listNode.HasChildNodes) { return null; }
            XmlNodeList zones = listNode.SelectNodes(".//Section");
            if (zones.Count == 0) { return null; }

            ListToken token = new ListToken();
            foreach (XmlNode zone in zones)
            {
                // LIST
                XmlNode beginNode = zone.SelectSingleNode("Begin");
                if (beginNode == null) { continue; }
                XmlAttribute attIndicesBegin = beginNode.Attributes["index"];
                int[] indicesBegin = (attIndicesBegin == null) ? Array.Empty<int>() : ArrayFromNumList(attIndicesBegin.Value);
                XmlAttribute isBeginCaseSensitive = beginNode.Attributes["casing"];
                bool casingBegin = (isBeginCaseSensitive == null) || (bool.TryParse(isBeginCaseSensitive.Value, out var beginSpecifiesCasing) ? beginSpecifiesCasing : true);

                XmlNode endNode = zone.SelectSingleNode("End");
                if (endNode == null) { continue; }
                XmlAttribute attIndicesEnd = endNode.Attributes["index"];
                int[] indicesEnd = (attIndicesEnd == null) ? Array.Empty<int>() : ArrayFromNumList(attIndicesEnd.Value);
                XmlAttribute isEndCaseSensitive = beginNode.Attributes["casing"];
                bool casingEnd = (isEndCaseSensitive == null) || (bool.TryParse(isEndCaseSensitive.Value, out var endSpecifiesCasing) ? endSpecifiesCasing : true);

                token.Sections.Add(new Section(beginNode.InnerText, endNode.InnerText, indicesBegin, indicesEnd, casingBegin, casingEnd));
            }

            // ELEMENT
            listNode = node.SelectSingleNode(elementNodeName);
            if (listNode == null) { return null; }
            token.Element = CreateTextToken(listNode);

            return token;
        }

        private ConditionalToken CreateConditionalToken(XmlNode node)
        {
            if (node.ChildNodes == null || node.ChildNodes.Count == 0) { return null; }

            Token textToken = CreateTextToken(node);
            if (textToken == null) { return null; }

            ConditionalToken cToken = new ConditionalToken(textToken);

            XmlAttribute att = node.Attributes["type"];
            if (att != null) { cToken.Type = att.Value; }

            XmlNode resultNode = node.SelectSingleNode("ResultTrue");
            cToken.ResultTrue = (resultNode == null) ? null : CreateTextToken(resultNode);

            resultNode = node.SelectSingleNode("ResultFalse");
            cToken.ResultFalse = (resultNode == null) ? null : CreateTextToken(resultNode);

            // EQUAL - OR
            XmlNodeList conditions = node.SelectNodes(".//Equal");
            CreateConditions(conditions, ref cToken);

            // NOT EQUAL - AND
            conditions = node.SelectNodes(".//NotEqual");
            CreateConditions(conditions, ref cToken);

            // CONTAINS - OR
            conditions = node.SelectNodes(".//Contains");
            CreateConditions(conditions, ref cToken);

            return cToken;
        }

        private void CreateConditions(XmlNodeList conditions, ref ConditionalToken cToken)
        {
            if (conditions == null || conditions.Count == 0) { return; }

            foreach (XmlNode condition in conditions)
            {
                Token conditionToken = CreateTextToken(condition);
                conditionToken.Name = condition.Name;

                XmlAttribute att = condition.Attributes["op"];
                if (att != null)
                {
                    conditionToken.Operator = att.Value;
                }

                cToken.Conditions.Add(conditionToken);
            }
        }

        private Type_Token CreateTypeToken(XmlNode node)
        {
            if (node.ChildNodes == null || node.ChildNodes.Count == 0) { return null; }

            Type_Token resultToken = new Type_Token();

            GatherProps(ref resultToken, node);

            return resultToken;
        }

        private void GatherProps(ref Type_Token resultToken, XmlNode node)
        {
            Type_Token propToken = null;
            foreach (XmlNode propNode in node.ChildNodes)
            {
                XmlAttribute attType = propNode.Attributes["type"];

                propToken = new Type_Token(CreateTextToken(propNode))
                {
                    Name = propNode.Name,
                    Type = (attType == null) ? string.Empty : attType.Value
                };

                XmlNodeList typeNodes = propNode.SelectNodes("Type");
                if (typeNodes.Count > 0)
                {
                    GatherProps(ref propToken, typeNodes[0]);
                }

                resultToken.Properties.Add(propToken);
            }
        }

        private void GatherSections(ref Token token, XmlNodeList zones)
        {
            foreach (XmlNode zone in zones)
            {
                XmlNode beginNode = zone.SelectSingleNode("Begin");
                if (beginNode == null) { continue; }
                XmlAttribute attIndicesBegin = beginNode.Attributes["index"];
                int[] indicesBegin = (attIndicesBegin == null) ? Array.Empty<int>() : ArrayFromNumList(attIndicesBegin.Value.Trim());
                XmlAttribute isBeginCaseSensitive = beginNode.Attributes["casing"];
                bool casingBegin = (isBeginCaseSensitive == null) || (bool.TryParse(isBeginCaseSensitive.Value, out var beginSpecifiesCasing) ? beginSpecifiesCasing : true);

                XmlNode endNode = zone.SelectSingleNode("End");
                if (endNode == null) { continue; }
                XmlAttribute attIndicesEnd = endNode.Attributes["index"];
                int[] indicesEnd = (attIndicesEnd == null) ? Array.Empty<int>() : ArrayFromNumList(attIndicesEnd.Value.Trim());
                XmlAttribute isEndCaseSensitive = beginNode.Attributes["casing"];
                bool casingEnd = (isEndCaseSensitive == null) || (bool.TryParse(isEndCaseSensitive.Value, out var endSpecifiesCasing) ? endSpecifiesCasing : true);

                Section section = new Section(beginNode.InnerText, endNode.InnerText, indicesBegin, indicesEnd, casingBegin, casingEnd);

                XmlNode altNode = zone.SelectSingleNode("Alt");
                if (altNode != null)
                {
                    beginNode = altNode.SelectSingleNode("Begin");
                    if (beginNode == null) { continue; }

                    endNode = altNode.SelectSingleNode("End");
                    if (endNode == null) { continue; }

                    section.AlternateSection = new Section(beginNode.InnerText, endNode.InnerText, indicesBegin, indicesEnd, true, true);
                }

                token.Sections.Add(section);
            }
        }

        public Token CreateTextToken(XmlNode node)
        {
            if (node == null) { return null; }

            if (!node.HasChildNodes) { return null; }
            XmlNodeList zones = node.SelectNodes("Section");

            Token token = new Token();

            // SECTIONS
            GatherSections(ref token, zones);
            zones = node.SelectNodes("Split");
            XmlAttribute att;
            if (zones != null && zones.Count > 0)
            {
                for (int i = 0; i < zones.Count; i++)
                {
                    // 0 = first element of split array only,  0,1 = first and second element,  0,2 = first and third element etc.
                    att = zones[i].Attributes["index"];
                    XmlAttribute attExclusion = zones[i].Attributes["exclude"];

                    // SPLIT
                    if (attExclusion == null || attExclusion.Value.Trim().Length == 0 || !bool.TryParse(attExclusion.Value.Trim(), out bool excludeIndices)) { excludeIndices = false; }
                    XmlNode delimNode = zones[i].SelectSingleNode("Delimiter");
                    token.Splits.Add(new SplitInfo(new[] { zones[i].FirstChild.InnerText }, (att == null) ? "0" : att.Value, (delimNode == null) ? string.Empty : delimNode.InnerText, excludeIndices));
                }
            }

            // TAG REMOVALS
            XmlNode stringOptionsNode = node.SelectSingleNode("TrimTags");
            token.TrimTags = stringOptionsNode != null;
            if (token.TrimTags)
            {
                // < 0 = from right, > 0 = from left, == 0 = all
                att = stringOptionsNode.Attributes["count"];
                token.TrimTagCount = (att == null) ? 0 : StringToInt(att.Value);
            }

            // REPLACEMENTS
            zones = node.SelectNodes("Replace");

            if (zones != null && zones.Count > 0)
            {
                foreach (XmlNode replace in zones)
                {
                    stringOptionsNode = replace.SelectSingleNode("OldText");
                    if (stringOptionsNode == null) { continue; }
                    Replacement rep = new Replacement(stringOptionsNode.InnerText);

                    stringOptionsNode = replace.SelectSingleNode("NewText");
                    if (stringOptionsNode != null) { rep.NewText = stringOptionsNode.InnerText; }

                    token.Replacements.Add(rep);
                }
            }

            // TRIM
            stringOptionsNode = node.SelectSingleNode("Trim");
            if (stringOptionsNode != null)
            {
                // -1 = left side only, 0 = both sides, 1 = right side only
                att = stringOptionsNode.Attributes["index"];
                token.TrimZone = (att == null) ? "0" : att.Value;
                token.Trim = stringOptionsNode.InnerText.ToCharArray();
            }

            stringOptionsNode = node.SelectSingleNode("Prepend");
            if (stringOptionsNode != null) { token.Prepend = stringOptionsNode.InnerText; }

            stringOptionsNode = node.SelectSingleNode("Append");
            if (stringOptionsNode != null) { token.Append = stringOptionsNode.InnerText; }

            zones = node.SelectNodes("Extraction");

            // EXTRACTIONS
            List<string> expressions;
            if (zones != null && zones.Count > 0)
            {
                expressions = new List<string>(zones.Count);
                foreach (XmlNode extraction in zones)
                {
                    if (extraction.InnerText.Length == 0 || !IsValidRegex(extraction.InnerText)) { continue; }
                    expressions.Add(extraction.InnerText);
                }
                token.Extractions = expressions.ToArray();
            }

            // VALIDATIONS
            zones = node.SelectNodes("Validation");
            if (zones != null && zones.Count > 0)
            {
                expressions = new List<string>(zones.Count);
                foreach (XmlNode extraction in zones)
                {
                    if (extraction.InnerText.Length == 0 || !IsValidRegex(extraction.InnerText)) { continue; }
                    expressions.Add(extraction.InnerText);
                }
                token.Validations = expressions.ToArray();
            }

            // VALIDATION EXPRESSION
            stringOptionsNode = node.SelectSingleNode("ValidationExpression");
            if (stringOptionsNode != null && stringOptionsNode.InnerText.Length > 0 && IsValidRegex(stringOptionsNode.InnerText))
            {
                token.ValidationExpression = stringOptionsNode.InnerText;
            }

            // HTML DECODE
            stringOptionsNode = node.SelectSingleNode("HtmlDecode");
            token.DecodeHTML = stringOptionsNode != null;

            // URL DECODE
            stringOptionsNode = node.SelectSingleNode("UrlDecode");
            token.DecodeUrl = stringOptionsNode != null;

            return token;
        }

        private static bool IsValidRegex(string pattern)
        {
            bool result = true;

            try
            {
                Regex.IsMatch(string.Empty, pattern);
            }
            catch (ArgumentException err)
            {
                Console.WriteLine(err.Message);
                result = false;
            }

            return result;
        }

        private static int StringToInt(string source) => int.TryParse(source, out int result) ? result : 0;

        private static int[] ArrayFromNumList(string csList)
        {
            if (csList == null || csList.Length == 0) { return Array.Empty<int>(); }

            string[] elems = csList.Split(Constants.c_comma);

            if (elems.Length == 0) { return Array.Empty<int>(); }

            var indices = new List<int>(elems.Length);

            string tmp;
            string tmpAbs;

            for (int i = 0; i < elems.Length; i++)
            {
                tmp = elems[i].Trim();
                if (IsNumber(tmp) && int.TryParse(tmp, out int itmp)) { indices.Add(itmp); }

                tmpAbs = elems[i].Trim().Replace("-", "");
                if (IsNumber(tmpAbs) && int.TryParse(tmpAbs, out itmp)) { indices.Add(itmp * -1); }
            }

            return indices.ToArray();
        }

        private static bool IsNumber(string s)
        {
            var c = s.Replace(" ", "").ToCharArray();

            for (int i = 0; i < c.Length; i++)
            {
                if (!char.IsNumber(c[i])) { return false; }
            }

            return true;
        }
    }
}
