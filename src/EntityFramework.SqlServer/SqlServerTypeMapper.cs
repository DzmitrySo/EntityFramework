// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerTypeMapper : RelationalTypeMapper
    {
        private readonly RelationalTypeMapping _nvarcharmax = new RelationalTypeMapping("nvarchar(max)");
        private readonly RelationalTypeMapping _nvarchar450 = new RelationalSizedTypeMapping("nvarchar(450)", 450);
        private readonly RelationalTypeMapping _varbinarymax = new RelationalTypeMapping("varbinary(max)", DbType.Binary);
        private readonly RelationalTypeMapping _varbinary900 = new RelationalSizedTypeMapping("varbinary(900)", DbType.Binary, 900);
        private readonly RelationalTypeMapping _rowversion = new RelationalSizedTypeMapping("rowversion", DbType.Binary, 8);
        private readonly RelationalTypeMapping _int = new RelationalTypeMapping("int");
        private readonly RelationalTypeMapping _bigint = new RelationalTypeMapping("bigint");
        private readonly RelationalTypeMapping _bit = new RelationalTypeMapping("bit");
        private readonly RelationalTypeMapping _smallint = new RelationalTypeMapping("smallint");
        private readonly RelationalTypeMapping _tinyint = new RelationalTypeMapping("tinyint");
        private readonly RelationalTypeMapping _money = new RelationalTypeMapping("money");
        private readonly RelationalSizedTypeMapping _nchar = new RelationalSizedTypeMapping("nchar", DbType.StringFixedLength, 1);
        private readonly RelationalSizedTypeMapping _nvarchar = new RelationalSizedTypeMapping("nvarchar", 1);
        private readonly RelationalTypeMapping _varcharmax = new RelationalTypeMapping("varchar(max)", DbType.AnsiString);
        private readonly RelationalSizedTypeMapping _char = new RelationalSizedTypeMapping("char", DbType.AnsiStringFixedLength, 1);
        private readonly RelationalSizedTypeMapping _varchar = new RelationalSizedTypeMapping("varchar", DbType.AnsiString, 1);
        private readonly RelationalSizedTypeMapping _varbinary = new RelationalSizedTypeMapping("binary", DbType.Binary, 1);
        private readonly RelationalTypeMapping _datetime = new RelationalTypeMapping("smalldatetime");
        private readonly RelationalTypeMapping _datetime2 = new RelationalTypeMapping("datetime2", DbType.DateTime2);
        private readonly RelationalTypeMapping _double = new RelationalTypeMapping("float");
        private readonly RelationalTypeMapping _datetimeoffset = new RelationalTypeMapping("datetimeoffset");
        private readonly RelationalTypeMapping _real = new RelationalTypeMapping("real");
        private readonly RelationalTypeMapping _uniqueidentifier = new RelationalTypeMapping("uniqueidentifier");
        private readonly RelationalScaledTypeMapping _decimal = new RelationalScaledTypeMapping("decimal(18, 2)", 18, 2);
        private readonly RelationalTypeMapping _time = new RelationalTypeMapping("time");

        private readonly Dictionary<string, RelationalTypeMapping> _simpleNameMappings;
        private readonly Dictionary<Type, RelationalTypeMapping> _simpleMappings;

        public SqlServerTypeMapper()
        {
            _simpleNameMappings
                = new Dictionary<string, RelationalTypeMapping>(StringComparer.OrdinalIgnoreCase)
                    {
                        { "int", _int },
                        { "bigint", _bigint },
                        { "smallint", _smallint },
                        { "tinyint", _tinyint },
                        { "bit", _bit },
                        { "money", _money },
                        { "smallmoney", _money },
                        { "float", _double },
                        { "real", _real },
                        { "double precision", _double },
                        { "date", new RelationalTypeMapping("date") },
                        { "datetimeoffset", _datetimeoffset },
                        { "datetime2", _datetime2 },
                        { "smalldatetime", _datetime },
                        { "datetime", _datetime },
                        { "time", _time },
                        { "char", _char },
                        { "character", _char },
                        { "varchar", _varchar },
                        { "char varying", _varchar },
                        { "character varying", _varchar },
                        { "varchar(max)", _varcharmax },
                        { "char varying(max)", _varcharmax },
                        { "character varying(max)", _varcharmax },
                        { "nchar", _nchar },
                        { "national character", _nchar },
                        { "nvarchar", _nvarchar },
                        { "national char varying", _nvarchar },
                        { "national character varying", _nvarchar },
                        { "nvarchar(max)", _nvarcharmax },
                        { "national char varying(max)", _nvarcharmax },
                        { "national character varying(max)", _nvarcharmax },
                        { "text", _varchar },
                        { "ntext", _nvarchar },
                        { "binary", _varbinary },
                        { "varbinary", _varbinary },
                        { "binary varying", _varbinary },
                        { "varbinary(max)", _varbinarymax },
                        { "binary varying(max)", _varbinarymax },
                        { "image", _varbinarymax },
                        { "rowversion", _rowversion },
                        { "timestamp", _rowversion },
                        { "decimal", _decimal },
                        { "dec", _decimal },
                        { "numeric", _decimal }
                    };

            _simpleMappings
                = new Dictionary<Type, RelationalTypeMapping>
                    {
                        { typeof(int), _int },
                        { typeof(long), _bigint },
                        { typeof(DateTime), _datetime2 },
                        { typeof(Guid), _uniqueidentifier },
                        { typeof(bool), _bit },
                        { typeof(byte), _tinyint },
                        { typeof(double), _double },
                        { typeof(DateTimeOffset), _datetimeoffset },
                        { typeof(char), _int },
                        { typeof(sbyte), new RelationalTypeMapping("smallint") },
                        { typeof(ushort), new RelationalTypeMapping("int") },
                        { typeof(uint), new RelationalTypeMapping("bigint") },
                        { typeof(ulong), new RelationalTypeMapping("numeric(20, 0)") },
                        { typeof(short), _smallint },
                        { typeof(float), _real },
                        { typeof(decimal), _decimal },
                        { typeof(TimeSpan), _time }
                    };
        }

        protected override IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings
            => _simpleMappings;

        protected override IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings
            => _simpleNameMappings;

        protected override RelationalTypeMapping MapFromName(
            string typeName,
            string typeNamePrefix,
            int? firstQualifier,
            int? secondQualifier)
        {
            return TryMapSized(typeNamePrefix, new[] { "nvarchar", "national char varying", "national character varying" }, firstQualifier)
                   ?? TryMapSized(typeNamePrefix, new[] { "varbinary", "binary varying" }, firstQualifier, DbType.Binary)
                   ?? TryMapSized(typeNamePrefix, new[] { "varchar", "char varying", "character varying" }, firstQualifier, DbType.AnsiString)
                   ?? TryMapScaled(typeNamePrefix, new[] { "decimal", "numeric", "dec" }, firstQualifier, secondQualifier)
                   ?? TryMapScaled(typeNamePrefix, new[] { "float", "double precision" }, firstQualifier, secondQualifier)
                   ?? TryMapScaled(typeNamePrefix, new[] { "datetime2" }, firstQualifier, secondQualifier, DbType.DateTime2)
                   ?? TryMapScaled(typeNamePrefix, new[] { "datetimeoffset" }, firstQualifier, secondQualifier)
                   ?? TryMapSized(typeNamePrefix, new[] { "nchar", "national char", "national character" }, firstQualifier, DbType.AnsiStringFixedLength)
                   ?? TryMapSized(typeNamePrefix, new[] { "char", "character" }, firstQualifier, DbType.StringFixedLength)
                   ?? TryMapSized(typeNamePrefix, new[] { "binary" }, firstQualifier, DbType.Binary)
                   ?? base.MapFromName(typeName, typeNamePrefix, firstQualifier, secondQualifier);
        }

        protected override RelationalTypeMapping MapCustom(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapEnumType();

            return clrType == typeof(string)
                ? MapString(property, "nvarchar", _nvarcharmax, _nvarchar450)
                : clrType == typeof(byte[])
                    ? MapByteArray(property, "varbinary", _varbinarymax, _varbinary900, _rowversion)
                    : base.MapCustom(property);
        }
    }
}
