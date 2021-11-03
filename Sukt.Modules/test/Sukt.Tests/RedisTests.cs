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
using Sukt.Module.Core.Extensions;
using System.Collections.Generic;

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
                var user = new User() { Name = $"$���ɻ�һ��{i}" };
                var index = await _redisRepository.SetListLeftPushAsync("listleft_top_test_user", user.ToJson());
            }
            var result = await _redisRepository.GetListRangeAsync("listleft_top_test_user");
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
            var result = await _redisRepository.SetListLeftPushAsync(key, value);
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

                var result = await _redisRepository.SetListLeftPushAsync("listleft_last_test", $"{value}+++++++++++++++++{i}");
                Console.WriteLine(result);
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
            var result = await _redisRepository.SetListRightPushAsync(key, value);
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
        /// <summary>
        /// Hash��Ϊ���ﳵ����
        /// </summary>
        /// <returns></returns>
        [Fact]
        public async Task HashShoppingCar_Test()
        {
            var key = $"shoppingcar_333bfa6d-1917-4f0c-b7c1-bd7817c58521";
            var arr= await _redisRepository.GetHashListAsync(key);
            List<string> productids = new List<string>();
            for (int i = 0; i < 20; i++)
            {
                productids.Add(Guid.NewGuid().ToString());
            }
            //��Ӳ�Ʒ
            foreach (var item in productids)
            {
                var lockerkey = await _redisRepository.SetHashFieldAsync(key, item, "1");
            }
            //���ݲ�Ʒ����<1>�ۼƹ��ﳵ����
            foreach (var item in productids)
            {
                var lockerkey = await _redisRepository.IncrementHashFieldAsync(key, item);
            }
            //���ݲ�Ʒ�ʹ���������ۼƹ��ﳵ����
            foreach (var item in productids)
            {
                var lockerkey = await _redisRepository.IncrementHashFieldAsync(key, item, 5);
            }
        }
    }
    [SuktDependsOn(typeof(DependencyAppModule))]
    public class RedisModule : RedisModuleBase
    {
        public override void AddRedis(IServiceCollection service)
        {
            service.AddRedis("192.168.31.175:6379,password=P@ssW0rd,defaultDatabase=2,prefix=sukt_admin_");
        }
    }
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; }
    }
}
