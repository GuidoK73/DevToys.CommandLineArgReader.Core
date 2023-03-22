using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;

namespace DevToys.Core
{
    /// <summary>
    /// this class can be used to create standard console applications with an extensive set of parameters.
    /// </summary>
    /// <typeparam name="COMMANDLINEARGUMENTS">Object containing argument properties.</typeparam>
    public class CommandLineArgReader<COMMANDLINEARGUMENTS> where COMMANDLINEARGUMENTS : class, new()
    {
        private readonly Dictionary<string, PropertyInfo> _Properties = new();
        private Dictionary<string, bool> _RequiredPropertiesSet = new();
        private readonly Dictionary<string, string> _AltKeys = new();
        private readonly Dictionary<string, string> _AllKeys = new();
        private List<Argument> _Arguments = new();
        private readonly HashSet<string> _Hidden = new();
        private PropertyInfo _DefaultProperty;
        private readonly StringBuilder _SbErrorText = new();
        private const string _DefaultCharValue = "";
        private readonly string[] _Args;
        private string _helpText = string.Empty;

        private class Argument
        {
            public string Key { get; set; }
            public List<string> Values { get; set; } = new List<string>();
            public bool IsSwitch { get; set; } = false;
        }

        public bool HelpRequested { get; private set; }

        public bool NoArgs => (_Args == null || _Args.Length == 0);

        public static object Convert(object value, Type target)
        {
            target = Nullable.GetUnderlyingType(target) ?? target;
            return (target.IsEnum) ? Enum.Parse(target, value.ToString()) : ChangeType(value, target);
        }

        private static object ChangeType(object value, Type target)
        {
            if (target.IsArray && value.GetType().IsArray == false)
            {
                string[] _array = new string[] { value.ToString() };
                return System.Convert.ChangeType(_array, target);
            }
            else
                return System.Convert.ChangeType(value, target);

        }

        public CommandLineArgReader(string[] args)
        {
            _Args = args;
        }

        private void Init()
        {
            if (_Properties.Count > 0)
                return;

            var _type = typeof(COMMANDLINEARGUMENTS);

            var _properties = _type.GetProperties()
                .Where(p => p.GetCustomAttribute(typeof(DataMemberAttribute), true) != null)
                .Select(p => new { Value = p, Key = (p.GetCustomAttribute(typeof(DataMemberAttribute), true) as DataMemberAttribute)?.Name })
                .ToDictionary(p => p.Key, p => p.Value);

            var _hidden = _type.GetProperties()
                .Where(p => (p.GetCustomAttribute(typeof(BrowsableAttribute)) as BrowsableAttribute)?.Browsable == false)
                .Select(p => (p.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute)?.Name.Split(new char[] { ',' }).First())
                .Where(p => !string.IsNullOrEmpty(p));

            foreach (var key in _hidden)
                _Hidden.Add(key);

            DefaultPropertyAttribute _defaultAttribute = _type.GetCustomAttribute(typeof(DefaultPropertyAttribute)) as DefaultPropertyAttribute;
            if (_defaultAttribute != null)
                _DefaultProperty = _type.GetProperty(_defaultAttribute.Name);

            foreach (var property in _properties)
            {
                var _keys = property.Key.Split(new char[] { ',' });
                var _key = string.Empty;
                for (int ii = 0; ii < _keys.Length; ii++)
                {
                    if (ii == 0)
                    {
                        _key = _keys[ii].Trim();
                        _AllKeys.Add(_key, property.Key);
                        _Properties.Add(_key, property.Value);
                    }
                    else
                    {
                        _AltKeys.Add(_keys[ii].Trim(), _key);
                    }
                }
            }
        }

        public virtual COMMANDLINEARGUMENTS GetObject()
        {
            Init();

            var _retValue = new COMMANDLINEARGUMENTS();
            var _previousIsKey = true;
            var _first = true;
            var _arg = new Argument();
            var _type = typeof(COMMANDLINEARGUMENTS);

            _RequiredPropertiesSet = _type.GetProperties()
                .Where(p => (p.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute)?.IsRequired == true)
                .Select(p => new { Key = (p.GetCustomAttribute(typeof(DataMemberAttribute)) as DataMemberAttribute).Name.Split(new char[] { ',' }).First(), Value = false })
                .ToDictionary(p => p.Key, p => p.Value);

            if (_Args.Length == 0)
                return _retValue;

            if (_Args.Length == 1)
            {
                if (_Args[0] == "/?" || _Args[0] == "/help")
                    HelpRequested = true;

                if (!IsKey(0) && _DefaultProperty != null)
                {
                    FillProperty(_retValue, _DefaultProperty, _Args[0]);
                    return _retValue;
                }
            }

            _Arguments = new List<Argument>();

            for (int ii = 0; ii < _Args.Length; ii++)
            {

                bool _isKey = IsKey(ii);
                if (_isKey && !_previousIsKey)
                {
                    _Arguments.Add(_arg);
                    _arg = new Argument();
                }
                if (_isKey && _previousIsKey && !_first)
                {
                    _arg.IsSwitch = true;
                    _Arguments.Add(_arg);
                    _arg = new Argument();
                }

                if (_isKey)
                    _arg.Key = GetRealKey(_Args[ii]);

                if (!_isKey)
                    _arg.Values.Add(_Args[ii]);

                _previousIsKey = _isKey;
                _first = false;
            }
            if (_previousIsKey)
                _arg.IsSwitch = true;

            _Arguments.Add(_arg);

            foreach (Argument arg in _Arguments)
            {
                if (string.IsNullOrEmpty(arg.Key))
                {
                    HelpRequested = true;
                    return _retValue;
                }

                _RequiredPropertiesSet[arg.Key] = true;
                if (arg.IsSwitch)
                {
                    FillProperty(_retValue, arg.Key, "true");
                    continue;
                }
                if (arg.Values.Count == 1)
                {
                    FillProperty(_retValue, arg.Key, arg.Values[0]);
                    continue;
                }
                if (arg.Values.Count > 1)
                {
                    FillProperty(_retValue, arg.Key, arg.Values);
                    continue;
                }
            }

            foreach (var required in _RequiredPropertiesSet.Where(p => p.Value == false))
            {
                _SbErrorText.Append($"Required key {required.Key} not set.\r\n");
                HelpRequested = true;
            }

            return _retValue;
        }

        private string GetRealKey(string key) => _AltKeys.ContainsKey(key) ? _AltKeys[key] : key;

        private bool IsKey(int index) => (index >= _Args.Length) ? false : (_AltKeys.ContainsKey(_Args[index]) || _Properties.ContainsKey(_Args[index]));

        private void FillProperty(COMMANDLINEARGUMENTS subject, string key, string value)
        {
            var _propInfo = _Properties[key];
            FillProperty(subject, _propInfo, value);
        }

        private void FillProperty(COMMANDLINEARGUMENTS subject, PropertyInfo propInfo, string value)
        {
            if (string.IsNullOrEmpty(value))
                propInfo.SetValue(subject, _DefaultCharValue);
            else
                propInfo.SetValue(subject, Convert(value, propInfo.PropertyType));
        }

        private void FillProperty(COMMANDLINEARGUMENTS subject, string key, List<string> values)
        {
            var _propInfo = _Properties[key];
            var type = _propInfo.PropertyType;
            if (type.IsArray)
            {
                var _innerType = type.GetElementType();
                var _values = (Array)Activator.CreateInstance(type, new object[] { values.Count });
                for (int ii = 0; ii < values.Count; ii++)
                    _values.SetValue(Convert(values[ii], _innerType), ii);

                _propInfo.SetValue(subject, _values);
            }
        }

        public string Help
        {
            get
            {
                if (!string.IsNullOrEmpty(_helpText))
                    return _helpText;

                var _defaultName = string.Empty;
                var _sb = new StringBuilder();
                var _errorText = _SbErrorText.ToString();
                var type = typeof(COMMANDLINEARGUMENTS);

                if (_DefaultProperty != null)
                    _defaultName = _DefaultProperty.Name;

                if (!string.IsNullOrWhiteSpace(_errorText))
                    _sb.Append(_SbErrorText).Append("\r\n");

                _sb.Append($"Usage:\r\n\r\n");

                foreach (var x in _Properties)
                {
                    if (_Hidden.Contains(x.Key))
                        continue;

                    string _allKeys = _AllKeys[x.Key].Replace(" ", "").Replace(",", " | ");
                    _sb.Append($"{_allKeys} ");

                    if (x.Value.PropertyType.IsEnum)
                        _sb.Append($"[{ string.Join(" | ", Enum.GetNames(x.Value.PropertyType)) }] ");
                    else if (x.Value.PropertyType.IsArray)
                        _sb.Append($"[PARAM] [PARAM] [PARAM] etc. ");
                    else if (x.Value.PropertyType == typeof(bool))
                        _sb.Append(' ');
                    else
                        _sb.Append($"[{x.Value.Name}] ");

                    _sb.Append(_RequiredPropertiesSet.ContainsKey(x.Key) ? $"(Required) " : "");
                    _sb.Append(x.Value.Name.Equals(_defaultName) ? $"(Default)" : "");
                    _sb.Append((x.Value.PropertyType.IsArray) ? $"(Array, any following parameter not a keyword is assumed to be an array element.)" : "");
                    _sb.Append($"\r\n");

                    DescriptionAttribute _attribute = x.Value.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                    if (_attribute != null)
                        _sb.Append($"{_attribute.Description}\r\n");

                    _sb.Append($"\r\n ");
                }

                DescriptionAttribute _classDescription = type.GetCustomAttribute(typeof(DescriptionAttribute)) as DescriptionAttribute;
                _sb.Append((_classDescription != null) ? $"\r\n\r\n{_classDescription.Description}\r\n" : "");
                _helpText = _sb.ToString();
                return _helpText;
            }
        }
    }
}
