using Newtonsoft.Json;
using System.IO;

namespace CityGen.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var generator = new Generator(1806, 1000, 1000, new[] { new Segment(new Vector(0, 0), new Vector(5, 0), 0), new Segment(new Vector(5,0), new Vector(5,5),0), new Segment(new Vector(5,5), new Vector(0,5), 0), new Segment(new Vector(0,5), new Vector(0,0), 0) })
            {
                MinAngle = -90f,
                MaxAngle = 90f,
                AngleSteps = 90f,
                MinNewSegments = 2,
                MaxNewSegments = 3,
                MinSegmentLength = 5,
                SegmentStepLength = 5,
                MaxSegmentLength = 30,
            };

            using (FileStream fs = File.Open(@"person.js", FileMode.Create))
            using (StreamWriter sw = new StreamWriter(fs))
            using (JsonWriter jw = new JsonTextWriter(sw))
            {
                sw.Write("var segments = [");
                jw.Formatting = Formatting.Indented;

                JsonSerializer serializer = new JsonSerializer();

                for (var i = 0; i < 15; ++i)
                {
                    generator.DoStep();
                    serializer.Serialize(jw, generator.Segments);
                    sw.Write(",");
                }

                sw.Write("]");
            }
        }
    }
}
