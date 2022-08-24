using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Data.Serializers
{
    static class Extensions
    {
        #region Type Reflection
        public static Type GetType(string Name, string NameSpace = null)
        {
            if (NameSpace != null)
                Name = NameSpace + "." + Name;

            Type type = Type.GetType(Name);
            if (type != null) return type;

            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                type = asm.GetType(Name);
                if (type != null) return type;
            }

            return null;
        }
        public static FieldInfo[] GetHierarchicalFields(this Type type, BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
        {
            FieldInfo[] fieldInfos = type.GetFields(bindingFlags);
            var fieldInfoList = new List<FieldInfo>();

            //function declaration
            Action filterAdd = () =>
            {
                foreach (FieldInfo f in fieldInfos)
                {
                    if (f.FieldType != typeof(EventHandler) && !f.FieldType.IsSubclassOf(typeof(EventHandler)))
                    {
                        if (!fieldInfoList.Contains(f))
                            fieldInfoList.Add(f);
                    }
                }
            };

            //first time removal
            filterAdd();

            // If this class doesn't have a base, don't waste any time
            if (type.BaseType == typeof(object))
            {
                return fieldInfoList.ToArray();
            }
            else
            {   // Otherwise, collect all types up to the furthest base class
                var currentType = type;


                while (currentType != typeof(object))
                {
                    fieldInfos = currentType.GetFields(bindingFlags);
                    filterAdd();
                    currentType = currentType.BaseType;
                }
                return fieldInfoList.ToArray();
            }
        }

        //----------------------------
        internal static HashSet<Tuple<string, MemberInfo>> CacheKnownType<Attr>(Type type, Dictionary<Type, HashSet<Tuple<string, MemberInfo>>> _cache) where Attr : Attribute
        {
            HashSet<Tuple<string, MemberInfo>> lsf;

            //return cache
            if (_cache.TryGetValue(type, out lsf))
                return lsf;

            //-- start cache
            //get all fields of type
            FieldInfo[] finfo = type.GetHierarchicalFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            lsf = new HashSet<Tuple<string, MemberInfo>>();
            Dictionary<string, FieldInfo> dicf = new Dictionary<string, FieldInfo>();

            //map StreamableJsonMember Fields
            foreach (FieldInfo f in finfo)
            {
                dicf.Add(f.Name, f); //prepare key
                lsf.Add(new Tuple<string, MemberInfo>(f.Name, f));
            }

            //cache
            _cache.Add(type, lsf);
            return lsf;
        }
        #endregion
    }
}
