//using Quartz.Impl;
//using Quartz;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace PersonalFinanceManager.Sheduler.Host
//{
//    class QuartzTest
//    {
//        static async Task Main()
//        {
//            var schedulerFactory = new StdSchedulerFactory();
//            var scheduler = await schedulerFactory.GetScheduler();
//            var job = JobBuilder.Create<SimpleJob>()
//                .WithIdentity("testJob", "group1")
//                .Build();
//            var trigger = TriggerBuilder.Create()
//                .WithIdentity("testTrigger", "group1")
//                .StartNow()
//                .WithSimpleSchedule(x => x.WithIntervalInSeconds(5).RepeatForever())
//                .Build();
//            await scheduler.ScheduleJob(job, trigger);
//            await scheduler.Start();
//            await Task.Delay(10000);
//            await scheduler.Shutdown();
//        }
//    }

//    class SimpleJob : IJob
//    {
//        public async Task Execute(IJobExecutionContext context)
//        {
//            Console.WriteLine("SimpleJob running at: " + DateTime.Now);
//            await Task.CompletedTask;
//        }
//    }
//}
