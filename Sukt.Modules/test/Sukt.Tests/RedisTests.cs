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
            await _redisRepository.SetStringAsync("test", source, TimeSpan.FromMinutes(20));
            var target = await _redisRepository.GetStringAsync("test");
            target.ShouldBe(source);
            await _redisRepository.RemoveAsync("test");
        }
        /// <summary>
        /// ��Listͷ��ѭ������ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListLeftTopPush_Test()
        {
            var value = "Sukt.Core.Top";
            for (int i = 0; i < 80; i++)
            {
                await _redisRepository.SetListLeftPushAsync("listleft_top_test", $"{value }---------------------{i}");
            }
        }
        /// <summary>
        /// Listͷ�������ȡ��ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetListLeftTopPop_Test()
        {
            var value = "Sukt.Core";
            var key = "list_left_top_insert";
            var result = await _redisRepository.SetListLeftPushAsync(key,value);
            var target = await _redisRepository.GetListLeftPopAsync(key);
            target.ShouldBe(value);
        }
        /// <summary>
        /// ��Listβ��ѭ������ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task ListRightPush_Test()
        {
            var value = "Sukt.Core.Last";
            for (int i = 0; i < 80; i++)
            {

                await _redisRepository.SetListLeftPushAsync("listleft_last_test", $"{value}+++++++++++++++++{i}");
            }
        }
        /// <summary>
        /// Listβ�������ȡ��ֵ
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task GetListRightPush_Test()
        {
            var value = "Sukt.Core";
            var key = "list_left_last_insert";
            var result = await _redisRepository.SetListRightPushAsync(key,value);
            var target = await _redisRepository.GetListRightPopAsync(key);
            target.ShouldBe(value);
        }
        /// <summary>
        /// �ֲ�ʽ����Ԫ����
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task DistributedLocker_Test()
        {
            var key = "miaoshakoujiankucun";
            var lockerkey = await _redisRepository.LockAsync(key, TimeSpan.FromSeconds(15));
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
            service.AddRedis("192.168.31.144:6379,password=P@ssW0rd,defaultDatabase=5,prefix=sukt_admin_");
        }
    }
}
