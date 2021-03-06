﻿// Copyright (c) Josef Pihrt. All rights reserved. Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.ObjectModel;
using System.Xml.Linq;
using Pihrtsoft.Records.Utilities;

namespace Pihrtsoft.Records
{
    internal class BaseRecordReader : AbstractRecordReader
    {
        public BaseRecordReader(XElement element, EntityDefinition entity, DocumentSettings settings)
            : base(element, entity, settings)
        {
        }

        private ExtendedKeyedCollection<string, Record> Records { get; set; }

        public override Collection<Record> ReadRecords()
        {
            Collect(Element.Elements());

            return Records;
        }

        protected override void AddRecord(Record record)
        {
            if (Records == null)
            {
                Records = new ExtendedKeyedCollection<string, Record>();
            }
            else if (Records.Contains(record.Id))
            {
                Throw(ErrorMessages.ItemAlreadyDefined(PropertyDefinition.IdName, record.Id));
            }

            Records.Add(record);
        }

        protected override Record CreateRecord(string id)
        {
            if (id == null)
                Throw(ErrorMessages.MissingBaseRecordIdentifier());

            return new Record(Entity, id);
        }
    }
}
