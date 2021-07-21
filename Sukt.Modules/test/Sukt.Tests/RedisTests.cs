using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Sukt.Module.Core.Modules;
using Sukt.Module.Core.SuktDependencyAppModule;
using Sukt.Redis;
using Sukt.TestBase;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Sukt.Tests
{
    /// <summary>
    /// Redis ��Ԫ����
    /// </summary>
    public class RedisTests : IntegratedTest<RedisModule>
    {
        private readonly IRedisRepository _redisRepository;

        public RedisTests()
        {
            _redisRepository = ServiceProvider.GetService<IRedisRepository>();
        }
        /// <summary>
        /// д���ַ���
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task String_Test()
        {
            var source = "adsadadasda";
            await _redisRepository.SetAsync("test", source, TimeSpan.FromMinutes(20));
            var target = await _redisRepository.GetStringAsync("test");
            target.ShouldBe(source);
            await _redisRepository.RemoveAsync("test");
        }
        /// <summary>
        /// ��Listͷ������ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListLeftPush_Test()
        {
            var source = "123456";
            await _redisRepository.SetListLeftPushAsync("listleft_test", source);
            var target = await _redisRepository.GetListLeftPopAsync("listleft_test");
            target.ShouldBe(source);
        }
        /// <summary>
        /// ��Listβ������ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListRightPush_Test()
        {
            var source = "123456";
            await _redisRepository.SetListLeftPushAsync("listleft_test", source);
            var target = await _redisRepository.GetListLeftPopAsync("listleft_test");
            target.ShouldBe(source);
        }
        /// <summary>
        /// �ֲ�ʽ����Ԫ����
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DistributedLocker_Test()
        {
            var key = "Order002";
            var lockerkey = await _redisRepository.LockAsync(key, TimeSpan.FromSeconds(180));
            try
            {
                if (!lockerkey)
                {
                    //δ��ȡ�����ڴ˴������쳣��Ϣ
                }
                else
                {
                    //δ��ȡ����
                    lockerkey.ShouldBe(true);
                    Console.WriteLine("��ȡ������");
                    //Thread.Sleep(1000);//˯��һ��ʱ�䣬ģ��ҵ�����
                }
            }
            finally
            {
                //�м�Ҫ��finally�ͷ���
                var result = await _redisRepository.UnLockAsync(key);
                result.ShouldBe(true);
            }
        }
    }
    [SuktDependsOn(typeof(DependencyAppModule))]
    public class RedisModule : RedisModuleBase
    {
        public override void AddRedis(IServiceCollection service)
        {
            service.AddRedis("192.168.0.166:6379,password = redis123,defaultDatabase=5,prefix = test_");
        }
    }
}
