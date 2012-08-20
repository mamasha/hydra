/*
Copyright (c) 2010-2012 Mamasha Knows, all rights reserved
This is proprietary source code of Mamasha Knows Ltd.
The contents of this file may not be disclosed to third parties, 
copied or duplicated in any form, in whole or in part.
*/

using System;
using System.Collections.Generic;
using System.Reflection;
using l4p.VcallModel.Utils.Impl;


namespace l4p.VcallModel.Utils
{
    class FillPropertiesOfException : VcallModelException
    {
        public FillPropertiesOfException() { }
        public FillPropertiesOfException(string message) : base(message) { }
        public FillPropertiesOfException(string message, Exception inner) : base(message, inner) { }
    }

    static class FillPropertiesOf<TDstConfig>
            where TDstConfig : new()
    {
        public static TDstConfig From<TSrcConfig>(TSrcConfig srcConfig)
        {
            var config = new TDstConfig();

            Type srcType = typeof(TSrcConfig);
            Type dstType = typeof(TDstConfig);

            PropertyTree<TSrcConfig> srcTree;
            PropertyTree<TDstConfig> dstTree;

            try
            {
                srcTree = new PropertyTree<TSrcConfig>(srcConfig);
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Failed to parse properties of type '{0}'", srcType.Name);
                throw new FillPropertiesOfException(errMsg, ex);
            }

            try
            {
                dstTree = new PropertyTree<TDstConfig>(config);
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Failed to parse properties of type '{0}'", dstType.Name);
                throw new FillPropertiesOfException(errMsg, ex);
            }

            try
            {
                foreach (var property in dstTree.Properties)
                {
                    property.SetValue(srcTree[property.Name]);
                }
            }
            catch (Exception ex)
            {
                string errMsg = String.Format("Failed to copy properties of type '{0}' to '{1}'", srcType.Name, dstType.Name);
                throw new FillPropertiesOfException(errMsg, ex);
            }

            return config;
        }
    }
}

namespace l4p.VcallModel.Utils.Impl
{
    interface IProperty
    {
        string Name { get; }
        object Value { get; }
        void SetValue(object value);
    }

    class Property : IProperty
    {
        #region members

        private readonly PropertyInfo _info;
        private readonly object _instance;

        #endregion

        #region construction

        public Property(PropertyInfo info, object instance)
        {
            _info = info;
            _instance = instance;
        }

        #endregion

        #region IProperty

        string IProperty.Name
        {
            get { return _info.Name; }
        }

        object IProperty.Value
        {
            get { return _info.GetValue(_instance, null); }
        }

        void IProperty.SetValue(object value)
        {
            try
            {
                _info.SetValue(_instance, value, null);
            }
            catch (Exception ex)
            {
                Type dstType = _info.PropertyType;
                string errMsg;

                if (value != null)
                {
                    Type srcType = value.GetType();
                    errMsg = String.Format("Failed to set value of '{0}' property; from='{1}' to='{2}'", _info.Name, srcType.Name, dstType.Name);
                }
                else
                {
                    errMsg = String.Format("Failed to set null value to '{0}' property; to='{1}'", _info.Name, dstType.Name);
                }

                throw
                    new FillPropertiesOfException(errMsg, ex);
            }
        }

        #endregion
    }

    class PropertyTree<TConfig>
    {
        #region members

        private static readonly List<Type> _primitives = new List<Type>
        {
            typeof(bool), typeof(bool?),
            typeof(int), typeof(int?),
            typeof(long), typeof(long?),
            typeof(double), typeof(double?),
            typeof(string), typeof(string),
            typeof(DateTime), typeof(DateTime?),
            typeof(TimeSpan), typeof(TimeSpan?)
        };

        private readonly IDictionary<string, IProperty> _properties;

        #endregion

        #region private

        private bool is_primitive(Type type)
        {
            if (_primitives.Contains(type))
                return true;

            if (type.FullName.StartsWith("l4p.") == false)
                return true;

            return false;
        }

        private void add_property(PropertyInfo info, object obj)
        {
            var property = new Property(info, obj);
            string name = info.Name;

            if (_properties.ContainsKey(name))
                throw new FillPropertiesOfException(String.Format("Same property name '{0}' is found", name));

            _properties[name] = property;
        }

        private void parse_properties(Object obj)
        {
            foreach (var info in obj.GetType().GetProperties())
            {
                if (is_primitive(info.PropertyType))
                {
                    add_property(info, obj);
                    continue;
                }

                object value = info.GetValue(obj, null);

                try
                {
                    parse_properties(value);
                }
                catch (Exception ex)
                {
                    string errMsg = String.Format("Failed to parse properties of field '{0}'", info.Name);
                    throw new FillPropertiesOfException(errMsg, ex);
                }
            }
        }

        #endregion

        #region api

        public PropertyTree(TConfig from)
        {
            _properties = new Dictionary<string, IProperty>();
            parse_properties(from);
        }

        public IEnumerable<IProperty> Properties
        {
            get { return _properties.Values; }
        }

        public object this[string name]
        {
            get
            {
                if (_properties.ContainsKey(name) == false)
                    throw new FillPropertiesOfException(String.Format("Property '{0}' is not found", name));

                return _properties[name].Value;
            }
        }

        #endregion
    }
}

