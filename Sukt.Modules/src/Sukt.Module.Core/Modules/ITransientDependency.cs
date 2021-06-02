﻿using Sukt.Module.Core.Attributes.Dependency;

namespace Sukt.Module.Core
{
    /// <summary>
    /// 实现此接口的类型将自动注册为<see cref="ServiceLifetime.Transient"/>模式
    /// </summary>
    [IgnoreDependency]
    public interface ITransientDependency
    {
    }
}