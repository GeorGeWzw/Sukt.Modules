﻿using Microsoft.EntityFrameworkCore.Metadata;
using Sukt.Module.Core.Exceptions;

namespace Sukt.EntityFrameworkCore.Extensions
{
    public static class MutableEntityTypeExtension
    {
        public static void AddProperty<T>(this IMutableEntityType mutableEntityType, string propertyName)
        {
            if (mutableEntityType.FindProperty(propertyName) != null)
            {
                throw new SuktAppException($"属性冲突，无法添加{propertyName}的隐藏属性。");
            }
            mutableEntityType.AddProperty(propertyName, typeof(T));
        }
    }
}
