// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Xml.Linq;
using Pihrtsoft.Records.Utilities;

namespace Pihrtsoft.Records
{
    [DebuggerDisplay("Name = {Name,nq} DefaultValue = {DefaultValue} IsCollection = {IsCollection}, IsRequired = {IsRequired}")]
    public class PropertyDefinition : IKey<string>
    {
        internal PropertyDefinition(
            string name,
            bool isCollection = false,
            bool isRequired = false,
            object defaultValue = null,
            XElement element = null)
        {
            if (!object.ReferenceEquals(name, IdName)
                && !object.ReferenceEquals(name, TagsName))
            {
                if (DefaultComparer.NameEquals(name, IdName))
                    ThrowHelper.ThrowInvalidOperation(ErrorMessages.PropertyNameIsReserved(IdName), element);

                if (DefaultComparer.NameEquals(name, TagsName))
                    ThrowHelper.ThrowInvalidOperation(ErrorMessages.PropertyNameIsReserved(TagsName), element);
            }

            Name = name;
            IsCollection = isCollection;
            IsRequired = isRequired;
            DefaultValue = defaultValue;
        }

        internal static string IdName { get; } = "Id";

        internal static string TagsName { get; } = "Tags";

        internal static PropertyDefinition Id { get; } = new PropertyDefinition(IdName);

        internal static PropertyDefinition Tags { get; } = new PropertyDefinition(TagsName, isCollection: true);

        public string Name { get; }

        public bool IsCollection { get; }

        public bool IsRequired { get; }

        public object DefaultValue { get; }

        string IKey<string>.GetKey()
        {
            return Name;
        }
    }
}
