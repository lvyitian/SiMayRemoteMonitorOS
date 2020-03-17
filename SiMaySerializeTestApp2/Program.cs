using SiMay.Serialize;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace SiMaySerializeTestApp
{
    class Program
    {
        public class TestB
        {
            public bool[] isArray { get; set; }
            public bool isSuccess { get; set; }
            public byte[] Data { get; set; }
            public int Id { get; set; }
        }
        public class TestA
        {
            public int Id { get; set; }
            public int[] Ids { get; set; }
            public string Name { get; set; }
            public string[] Names { get; set; }
            public TestB B { get; set; }
            public TestA[] As { get; set; }
            public DateTime Time { get; set; }
            public MyEnum MyEnum { get; set; }
            public double[] bs { get; set; } = new double[] { 123, 132 };
        }
        public enum MyEnum
        {
            A, B, C
        }
        static void Main(string[] args)
        {
            List<TestA> list = new List<TestA>();
            for (int i = 0; i < 5; i++)
            {
                TestA a = new TestA()
                {
                    Id = 12313213 + i,
                    Ids = new int[] { 1213, 11, 1 },
                    Name = "哈哈哈",
                    Names = new string[] { "嘿嘿", "AAA" },
                    B = new TestB()
                    {
                        isSuccess = true,
                        Id = 12132132 + i,
                        Data = new byte[] { 1, 2, 255 }
                    },
                    As = new TestA[] { },
                    Time = DateTime.Now
                };
                list.Add(a);
            }
            TestA A = new TestA()
            {
                Id = 123,
                Ids = null,
                Name = null,
                Names = null,
                B = null,
                As = list.ToArray(),
                //Time = DateTime.Now,
                MyEnum = MyEnum.B
            };

            Stopwatch sw = new Stopwatch();
            sw.Start();
            var bytes = PacketSerializeHelper.SerializePacket(A);
            sw.Stop();
            Console.WriteLine("序列化耗时:" + sw.Elapsed.TotalSeconds);
            sw.Reset();
            sw.Start();
            TestA pack = PacketSerializeHelper.DeserializePacket<TestA>(bytes);
            sw.Stop();
            Console.WriteLine("反序列化耗时:" + sw.Elapsed.TotalSeconds);

            Console.Read();
        }
    }
}
