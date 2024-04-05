﻿using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace Simple.Dapper
{
    /// <summary>表标签</summary>
    public class DbTableAttribute : Attribute
    {
        public DbTableAttribute(TableAttribute table)
        {
            Name = table.Name;
            Schema = table.Schema;
            Alias = "T0";
        }

        public DbTableAttribute(string name, string schema = "", string alias = "T0")
        {
            Name = name;
            Schema = schema;
            Alias = alias;
        }

        public string Name { get; set; }
        public string Schema { get; set; }
        public string Alias { get; set; }
    }

    /// <summary>列标签</summary>
    public class DbColumnAttribute : Attribute
    {
        public DbColumnAttribute(ColumnAttribute column)
        {
            Name = column.Name;
        }

        public DbColumnAttribute(string name = "", string description = "", bool isIdentityKey = false,
            bool canSelect = true, bool canInsert = true, bool canUpdate = true, bool canExport = true)
        {
            Name = name;
            Description = description;
            CanSelect = canSelect;
            CanInsert = canInsert;
            CanUpdate = canUpdate;
            CanExport = canExport;
            IsIdentityKey = isIdentityKey;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public bool IsKey { get; set; } = false;

        /// <summary>是否自增主键</summary>
        public bool IsIdentityKey { get; set; } = false;

        public bool CanSelect { get; set; } = true;
        public bool CanInsert { get; set; } = true;
        public bool CanUpdate { get; set; } = true;
        public bool CanExport { get; set; } = true;

        public bool IsString { get; set; } = false;

        /// <summary>是否可以为null</summary>
        public bool IsNullable { get; set; } = false;

        /// <summary>与数据库对应的数据类型</summary>
        public DbType? DataType { get; set; }

        public string PropertyName { get; set; }
        public PropertyInfo PropertyInfo { get; set; }

        /// <summary>是否忽略增删改查</summary>
        public bool Ignored { get; set; } = false;

        public object GetPropertyValue(object instance)
        {
            return PropertyInfo.GetValue(instance);
        }

        public void SetPropertyValue(object instance, object value)
        {
            PropertyInfo.SetValue(instance, value, null);
        }
    }
}