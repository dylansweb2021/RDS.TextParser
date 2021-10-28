using System;
using System.Text;
using System.Web;
using System.Text.Json;
using System.Reflection;
using System.Collections;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using RDS.TextParser.Interfaces;
using RDS.TextParser.Tokens;
using RDS.TextParser.Types;

namespace RDS.TextParser
{
    public class RDSTextParser : IRDSTextParser
    {
        private readonly SourceConfig config = new SourceConfig();

        public string ExceptionMessage = string.Empty;

        public object[][] GetResult(string configuration, string source)
        {
            ExceptionMessage = string.Empty;

            object[][] result;

            if (string.IsNullOrWhiteSpace(source)) { return Array.Empty<object[]>(); }

            try
            {
                if (!config.LoadConfigFromString(configuration)) { return Array.Empty<object[]>(); }

                result = new object[config.Tokens.Count][];

                var rows = new List<object[]>();

                var i = 0;

                foreach (var pair in config.Tokens)
                {
                    var handle = pair.Value.GetType().TypeHandle;

                    if (handle.Equals(Constants.handle_ListToken))
                    {
                        // TODO: handle ..
                    }
                    else if (handle.Equals(Constants.handle_TableToken))
                    {
                        result[i] = new object[] { GetTableResult(pair.Key, pair.Value as TableToken, source) };
                    }
                    else if (handle.Equals(Constants.handle_TypeListToken))
                    {
                        // TODO: handle ..
                    }
                    else
                    {
                        result[i] = new object[] { pair.Key, GetItem(pair.Value, source) };
                    }

                    i++;
                }
            }
            catch (Exception)
            {
                result = Array.Empty<object[]>();
            }

            return result;
        }

        public bool GetResult<T>(string configuration, string source, ref T result)
        {
            ExceptionMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(source)) { return false; }

            try
            {
                if (!config.LoadConfigFromString(configuration)) { return false; }

                result = GenerateResult<T>(config, source, result);

                return result != null;
            }
            catch (Exception err)
            {
                ExceptionMessage = $"[{DateTime.UtcNow:f}]  -  TYPE: {err.GetType().Name}  -  MESSAGE: {err.Message}";
                HandleError(err);
            }

            return false;
        }

        public Dictionary<string, object> GetResultDictionary(string tokens, string source)
        {
            return !config.LoadConfigFromString(tokens)
                ? new Dictionary<string, object>(0)
                : GetResultDictionaryUsingLoadedTokens(source);
        }

        private Table GetTableResult(string key, TableToken token, string source)
        {
            var result = new Table
            {
                Title = key
            };

            var elements = GetList(token, source);

            result.Rows = new List<string[]>(elements.Count);

            for (var h = 0; h < token.TypeToken.Properties.Count; h++)
            {
                result.Headers.Add(token.TypeToken.Properties[h].Name);
            }

            for (var i = 0; i < elements.Count; i++)
            {
                var cells = new List<string>(token.TypeToken.Properties.Count);

                for (var p = 0; p < token.TypeToken.Properties.Count; p++)
                {
                    cells.Add(GetItem(token.TypeToken.Properties[p], elements[i]));
                }

                result.Rows.Add(cells.ToArray());
            }

            return result;
        }

        private Dictionary<string, object> GetResultDictionaryUsingLoadedTokens(string source)
        {
            ExceptionMessage = string.Empty;

            Dictionary<string, object> result;

            if (string.IsNullOrWhiteSpace(source)) { return new Dictionary<string, object>(0); }

            try
            {
                result = new Dictionary<string, object>(config.Tokens.Count);

                var i = 0;

                foreach (var pair in config.Tokens)
                {
                    if (result.ContainsKey(pair.Key)) { continue; }

                    var handle = pair.Value.GetType().TypeHandle;

                    if (handle.Equals(Constants.handle_ListToken))
                    {
                        // TODO: implement/handle this
                    }
                    else if (handle.Equals(Constants.handle_TypeListToken))
                    {
                        var htmlElems = GetList((ListToken)pair.Value, source);

                        var listResults = new List<Dictionary<string, object>>(htmlElems.Count);

                        var tTokens = ((TypeListToken)pair.Value).TypeToken.Properties;

                        for (var elem = 0; elem < htmlElems.Count; elem++)
                        {
                            var ttResults = new Dictionary<string, object>(tTokens.Count);

                            for (var tt = 0; tt < tTokens.Count; tt++)
                            {
                                if (ttResults.ContainsKey(tTokens[tt].Name)) { continue; }

                                ttResults.Add(tTokens[tt].Name, GetItem(tTokens[tt], htmlElems[elem]));
                            }

                            listResults.Add(ttResults);
                        }

                        result.Add(pair.Key, listResults);
                    }
                    else
                    {
                        result.Add(pair.Key, GetItem(pair.Value, source));
                    }

                    i++;
                }
            }
            catch (Exception)
            {
                result = new Dictionary<string, object>(0);
            }

            return result;
        }

        private T GenerateResult<T>(SourceConfig config, string destinationHTML, T result)
        {
            var props = GetTypeProps(ref result);

            foreach (var prop in props)
            {
                if (config.Tokens.ContainsKey(prop.Key) && (config.Tokens[prop.Key] != null))
                {
                    var handle = config.Tokens[prop.Key].GetType().TypeHandle;

                    if (handle.Equals(Constants.handle_TypeListToken))
                    {
                        if (!TrySetValue(prop.Value, GetTypeList(Activator.CreateInstance(prop.Value.PropertyType.GetGenericArguments()[0]), (TypeListToken)config.Tokens[prop.Key], destinationHTML), ref result))
                        {
                            //ExceptionMessage = string.Format("Could not produce type correctly.");
                        }
                    }
                    else if (handle.Equals(Constants.handle_ListToken))
                    {
                        // TODO: handle ..
                    }
                    else if (handle.Equals(Constants.handle_TypeToken))
                    {
                        if (!TrySetValue(prop.Value, CreateArbitraryType(Activator.CreateInstance(prop.Value.PropertyType), (Type_Token)config.Tokens[prop.Key], destinationHTML), ref result))
                        {
                            //ExceptionMessage = string.Format("Could not produce type correctly.");
                        }
                    }
                    else
                    {
                        if (!TrySetValue(prop.Value, GetItem(config.Tokens[prop.Key], destinationHTML), ref result))
                        {
                            //ExceptionMessage = string.Format("Could not produce type correctly.");
                        }
                    }
                }
                else
                {
                    if (prop.Value.GetValue(result, null) == null)
                    {
                        if (!TrySetValue(prop.Value, string.Empty, ref result))
                        {
                            //ExceptionMessage = string.Format("Could not produce type correctly.");
                        }
                    }
                }
            }

            return (result == null) ? Activator.CreateInstance<T>() : result;
        }

        private static string GetItem_JSON(Token token, string destinationHTML)
        {
            string jsonStr = null;

            GatherZones(token, destinationHTML, ref jsonStr);

            if ((jsonStr == null) || (jsonStr.Length == 0)) { return string.Empty; }

            var pairs = GetJSONPairs(jsonStr);

            var jtoken = (JSONToken)token;

            return pairs[jtoken.Key].Value;
        }

        private static string GetItem(Token token, string destinationHTML)
        {
            if (token == null) { return string.Empty; }

            string result = null;

            if (token.GetType() == typeof(JSONToken))
            {
                result = GetItem_JSON(token, destinationHTML);
            }
            else
            {
                GatherZones(token, destinationHTML, ref result);

                CleanString(ref result);
            }

            // These ops need not occur if result is empty thus far...
            if (result.Length > 0)
            {
                // Regex and MatchCollection used by the following: Extractions, Validations and legacy ValidationExpression
                MatchCollection matches;

                var results = new List<string>(3);

                // EXTRACTIONS - Valid portions of result, retain these, drop the rest
                for (var i = 0; i < token.Extractions.Length; i++)
                {
                    matches = new Regex(token.Extractions[i]).Matches(result);

                    if (matches.Count == 0) { continue; }

                    for (var m = 0; m < matches.Count; m++)
                    {
                        results.Add(matches[m].Value);
                    }
                }

                // accumulate results...
                if (results.Count > 0) { result = string.Join(" ", results.ToArray()); }

                // TRIM TAGS
                if (token.TrimTags) { result = TrimTags(result, token.TrimTagCount); }

                // SPLIT
                if (token.Splits != null)
                {
                    for (var i = 0; i < token.Splits.Count; i++)
                    {
                        DoTheSplits(token.Splits[i], ref result);
                    }
                }

                // REPLACE
                if (token.Replacements.Count > 0)
                {
                    for (var i = 0; i < token.Replacements.Count; i++)
                    {
                        result = result.Replace(token.Replacements[i].OldText, token.Replacements[i].NewText);
                    }
                }

                // TRIM
                if (token.Trim != null)
                {
                    result = token.TrimZone switch
                    {
                        ("-1") => result.TrimStart(token.Trim),
                        ("1") => result.TrimEnd(token.Trim),
                        _ => result.Trim(token.Trim),
                    };
                }

                // VALIDATIONS - Retain or Drop in each case, exactly as <ValidationExpression> but multiple instances...
                for (var i = 0; i < token.Validations.Length; i++)
                {
                    matches = new Regex(token.Validations[i]).Matches(result);

                    if (matches.Count == 0) { result = string.Empty; break; }
                }

                // VALIDATION EXPRESSION - Retain or Drop - this must run, even if only against what was caused by the append and/or prepend...
                if (!string.IsNullOrEmpty(token.ValidationExpression))
                {
                    var expr = new Regex(token.ValidationExpression);
                    matches = expr.Matches(result);
                    if (matches.Count == 0) { return string.Empty; }
                    result = matches[0].Value;
                }
            }

            // These ops may be used against an empty result to produce a value...
            if (token.Prepend != null) { result = string.Concat(token.Prepend, result); }
            if (token.Append != null) { result = string.Concat(result, token.Append); }
            if (token.DecodeHTML) { result = HttpUtility.HtmlDecode(result); }
            if (token.DecodeUrl) { result = HttpUtility.UrlDecode(result); }

            return result;
        }

        private static string TrimTags(string source, int count)
        {
            var iOpening = source.IndexOf(Constants.c_lt);
            var iLeading = source.IndexOf(Constants.c_gt);
            var iClosing = source.LastIndexOf(Constants.c_gt);
            var iTrailing = source.LastIndexOf(Constants.c_lt);
            var trimLeading = (iOpening > -1) && (iLeading > -1) && (iLeading < iOpening);
            var trimTrailing = (iTrailing > -1) && (iClosing > -1) && (iTrailing > iClosing);
            var reverse = count < 0;

            if (trimLeading) { source = string.Concat("<", source); }
            if (trimTrailing) { source = string.Concat(source, ">"); }

            var exp = (reverse) ? new Regex(@"<(.|\n)*?>", RegexOptions.RightToLeft) : new Regex(@"<(.|\n)*?>");

            return (count == 0) ? exp.Replace(source, string.Empty).Trim() : exp.Replace(source, string.Empty, (reverse) ? count * -1 : count).Trim();
        }

        private static Dictionary<string, KeyValuePair<string, string>> GetJSONPairs(string jsonSource)
        {
            if (jsonSource.Length == 0) { return new Dictionary<string, KeyValuePair<string, string>>(); }

            var level1 = JsonSerializer.Deserialize<Dictionary<object, object>>(jsonSource);

            var info = new Dictionary<string, KeyValuePair<string, string>>();

            foreach (var item in level1)
            {
                var inner = item.Value as Dictionary<string, object>;

                string strKey = null;

                KeyValuePair<string, string> pair;

                if (inner != null)
                {
                    foreach (var innerPair in inner)
                    {
                        strKey = innerPair.Key;
                        pair = new KeyValuePair<string, string>(innerPair.Key, (innerPair.Value == null) ? string.Empty : innerPair.Value.ToString());
                        if (!info.ContainsKey(strKey)) { info.Add(strKey, pair); }
                    }
                }
                else
                {
                    strKey = item.Key.ToString();
                    pair = new KeyValuePair<string, string>(item.Key.ToString(), (item.Value == null) ? string.Empty : item.Value.ToString());
                    if (!info.ContainsKey(strKey)) { info.Add(strKey, pair); }
                }
            }

            return info;
        }

        private static void DoTheSplits(SplitInfo info, ref string result)
        {
            var strArr = info.Indices.Split(Constants.c_comma);
            var indices = new List<int>(strArr.Length);
            var isAllIndices = false;

            for (var i = 0; i < strArr.Length; i++)
            {
                if (strArr[i].Length == 0) { continue; }
                if (strArr[i].Equals("all")) { isAllIndices = true; break; }

                if (int.TryParse(strArr[i], out var index))
                {
                    indices.Add(index);
                }
            }

            // TODO: add preserveEmpties as recognized token node
            var preserveEmpties = false;
            strArr = result.Split(info.Split, (preserveEmpties) ? StringSplitOptions.None : StringSplitOptions.RemoveEmptyEntries);

            string[] parts;
            if (isAllIndices)
            {
                parts = strArr;
            }
            else
            {
                parts = new string[indices.Count];
                for (int i = 0; i < indices.Count; i++)
                {
                    parts[i] = (indices[i] < 0) ? ((strArr.Length + indices[i]) > -1) ? strArr[strArr.Length + indices[i]] : string.Empty : (indices[i] < strArr.Length) ? strArr[indices[i]] : string.Empty;
                }
            }

            result = string.Join(info.Delimiter, parts);
        }

        private static List<string> GetList(ListToken token, string destinationHTML)
        {
            var listZone = GetItem(token, destinationHTML);

            var elementStartStr = ((token.Element == null) || (token.Element.Sections.Count == 0))
                                      ? null
                                      : (token.Element.Sections.Count > 0)
                                          ? (((token.Element.Sections[0].Begin != null) && (token.Element.Sections[0].Begin.Length > 0))
                                                 ? token.Element.Sections[0].Begin[0]
                                                 : string.Empty)
                                          : null;

            List<string> retVal;
            if (elementStartStr == null)
            {
                var delimiter = GetItem(token.Element, destinationHTML);
                var results = listZone.Split(new string[] { delimiter }, StringSplitOptions.RemoveEmptyEntries);
                retVal = new List<string>(results);
            }
            else
            {
                retVal = new List<string>();

                if (elementStartStr.Length == 0) { return new List<string>(0); }

                var elementStartIndex = 0;

                do
                {
                    elementStartIndex = listZone.IndexOf(elementStartStr, elementStartIndex);
                    if (elementStartIndex == -1) { break; }
                    retVal.Add(GetItem(token.Element, listZone[elementStartIndex..]));
                    elementStartIndex += elementStartStr.Length;
                } while ((elementStartIndex > -1) && (elementStartIndex < listZone.Length));
            }

            return retVal;
        }

        private static List<T> GetTypeList<T>(T typeSample, TypeListToken token, string destinationHTML)
        {
            var retVal = new List<T>();

            var elements = GetList(token, destinationHTML);

            for (var i = 0; i < elements.Count; i++)
            {
                var loopVal = (T)Activator.CreateInstance(typeSample.GetType());
                retVal.Add(CreateArbitraryType<T>(loopVal, token.TypeToken, elements[i]));
            }

            return retVal;
        }

        private static bool GetConditional(ConditionalToken token, string destinationHTML, out string result)
        {
            if (token == null) { result = string.Empty; return false; }

            var retVal = false;
            var zoneItem = GetItem(token, destinationHTML);

            for (var i = 0; i < token.Conditions.Count; i++)
            {
                var item = GetItem(token.Conditions[i], destinationHTML);

                if (token.Conditions[i].Name.Equals("Equal"))
                {
                    if (token.Conditions[i].Operator.Equals("OR"))
                    {
                        retVal |= zoneItem.Equals(item);
                    }
                    else
                    {
                        retVal |= zoneItem.Equals(item);
                        if (!retVal) { break; }
                    }
                }
                else if (token.Conditions[i].Name.Equals("NotEqual"))
                {
                    if (token.Conditions[i].Operator.Equals("OR"))
                    {
                        retVal |= !zoneItem.Equals(item);
                    }
                    else
                    {
                        retVal |= !zoneItem.Equals(item);
                        if (!retVal) { break; }
                    }
                }
                else if (token.Conditions[i].Name.Equals("Contains"))
                {
                    retVal |= zoneItem.Contains(item);
                }
            }

            result = (retVal) ? GetItem(token.ResultTrue, destinationHTML) : GetItem(token.ResultFalse, destinationHTML);

            return retVal;
        }

        private static T CreateArbitraryType<T>(T typeSample, Type_Token tToken, string destinationHTML)
        {
            var props = GetTypeProps<T>(ref typeSample);

            for (var i = 0; i < tToken.Properties.Count; i++)
            {
                if (props.ContainsKey(tToken.Properties[i].Name))
                {
                    if (tToken.Properties[i].Properties.Count > 0)
                    {
                        var tmpObj = CreateArbitraryType(Activator.CreateInstance(props[tToken.Properties[i].Name].PropertyType), tToken.Properties[i], destinationHTML);
                        if (!TrySetValue<T>(props[tToken.Properties[i].Name], tmpObj, ref typeSample))
                        {
                            // TODO: handle .?
                            //return typeSample; 
                        }
                    }
                    else
                    {
                        if (!TrySetValue<T>(props[tToken.Properties[i].Name], GetItem(tToken.Properties[i], destinationHTML), ref typeSample))
                        {
                            // TODO: handle .?
                            //return typeSample; 
                        }
                    }
                }
            }

            return typeSample;
        }

        private static bool TrySetValue<T>(PropertyInfo info, object propVal, ref T result)
        {
            try
            {
                var val = ((propVal == null) || (propVal.ToString().Length == 0)) ? string.Empty : ConvertPropVal(propVal, info.PropertyType);
                info.SetValue(result, val, null);
                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            return false;
        }

        private static object ConvertPropVal(object s, Type t)
        {
            if (s == null || s.GetType() == Constants.t_string && s.ToString().Length == 0) { return null; }

            if (t.IsGenericType && s is IList)
            {
                var list = s as IList;
                var t_Element = t.GetGenericArguments()[0];

                var tList = typeof(List<>);
                var tArgs = new[] { t_Element };
                var tListOfT = tList.MakeGenericType(tArgs);
                var listOfT = (IList)Activator.CreateInstance(tListOfT, list.Count);

                foreach (var t1 in list)
                {
                    listOfT.Add(t1);
                }

                return listOfT;
            }

            try
            {
                return Convert.ChangeType(s, t);
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
            }

            if (t.Name.Equals("Uri"))
            {
                if (Uri.TryCreate(s.ToString(), UriKind.RelativeOrAbsolute, out var resUri))
                {
                    return resUri;
                }
            }
            else if (t.Name.Equals("DateTime"))
            {
                if (DateTime.TryParse(s.ToString(), out var resDT))
                {
                    return resDT;
                }

                var cInfo = CultureInfo.CreateSpecificCulture("en-US");
                if (DateTime.TryParse(s.ToString(), cInfo, DateTimeStyles.None, out resDT))
                {
                    return resDT;
                }
            }
            else if (t.IsEnum)
            {
                if (Enum.IsDefined(t, s))
                {
                    return Enum.Parse(t, s.ToString());
                }
            }

            return null;
        }

        private static void GatherZones(Token token, string sourceHTML, ref string result)
        {
            // ZONES - Start Result
            if (token.Sections.Count > 0)
            {
                if ((token.Sections[0].Begin.Length == 1) && (token.Sections[0].Begin[0].Length == 0))
                {
                    result = sourceHTML;
                }
                else
                {
                    result = GetBetween(sourceHTML, token.Sections[0].Begin, token.Sections[0].IndicesBegin, token.Sections[0].End, token.Sections[0].BeginIsCaseSensitive, token.Sections[0].EndIsCaseSensitive);

                    if ((token.Sections[0].AlternateSection != null) && (sourceHTML.Split(token.Sections[0].AlternateSection.Begin, StringSplitOptions.RemoveEmptyEntries).Length > 1))
                    {
                        result = GetBetween(sourceHTML, token.Sections[0].AlternateSection.Begin, token.Sections[0].AlternateSection.IndicesBegin, token.Sections[0].AlternateSection.End, true, true);
                    }

                    for (var i = 1; i < token.Sections.Count; i++)
                    {
                        if ((token.Sections[i].AlternateSection != null) && (result.Split(token.Sections[i].AlternateSection.Begin, StringSplitOptions.RemoveEmptyEntries).Length > 1))
                        {
                            result = GetBetween(result, token.Sections[i].AlternateSection.Begin, token.Sections[i].AlternateSection.IndicesBegin, token.Sections[i].AlternateSection.End, true, true);
                        }
                        else
                        {
                            result = GetBetween(result, token.Sections[i].Begin, token.Sections[i].IndicesBegin, token.Sections[i].End, token.Sections[i].BeginIsCaseSensitive, token.Sections[i].EndIsCaseSensitive);
                        }
                    }
                }
            }
            else
            {
                result = string.Empty;
            }
        }

        private static Dictionary<string, PropertyInfo> GetTypeProps<T>(ref T t)
        {
            var propInfos = t.GetType().GetProperties();

            var props = new Dictionary<string, PropertyInfo>(propInfos.Length);

            foreach (var t1 in propInfos)
            {
                if (!props.ContainsKey(t1.Name)) { props.Add(t1.Name, t1); }
            }

            return props;
        }

        private static string GetBetween(string source, string[] begin, int[] indicesBegin, string[] end, bool casingBegin, bool casingEnd)
        {
            var arr1 = casingBegin ? source.Split(begin, StringSplitOptions.None) : Regex.Split(source, string.Join("", begin), RegexOptions.IgnoreCase);

            var part2 = (arr1.Length < 2)
                            ? string.Empty
                            : arr1[((indicesBegin == null) || (indicesBegin.Length == 0))
                                       ? 1
                                       : ((indicesBegin[0] >= arr1.Length)
                                              ? 1
                                              : ((indicesBegin[0] < 0)
                                                     ? ((((indicesBegin[0] + arr1.Length) < 0) || ((indicesBegin[0] + arr1.Length) >= arr1.Length))
                                                            ? indicesBegin[0]
                                                            : indicesBegin[0] + arr1.Length)
                                                     : indicesBegin[0]))];

            var arr2 = casingEnd ? part2.Split(end, StringSplitOptions.None) : Regex.Split(part2, string.Join("", end), RegexOptions.IgnoreCase);

            return (arr2 == null || arr2.Length == 0) ? string.Empty : arr2[0];
        }

        private static void CleanString(ref string s)
        {
            var sb = new StringBuilder();

            var arr = s.ToCharArray();

            for (var i = 0; i < arr.Length; i++)
            {
                var skip = false;

                for (var c = 0; c < Constants._removals.Length; c++)
                {
                    if (arr[i].Equals(Constants._removals[c])) { skip = true; break; }
                }

                if (!skip) { sb.Append(arr[i]); }
            }

            s = sb.ToString();
        }

        private static void HandleError(Exception err)
        {
            Console.WriteLine(err.Message);
            throw err;
        }
    }
}
