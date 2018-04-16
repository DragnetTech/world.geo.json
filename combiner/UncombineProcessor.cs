using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using System.Linq;
using System.IO;
using Newtonsoft.Json.Linq;

namespace combiner
{
    public class UncombineProcessor
    {
        public static int Uncombine(Program.UncombineOptions options)
        {
            var combineFile = "combined.json";

            var combinedJsonText = System.IO.File.ReadAllText(combineFile);

            var combinedJson = JObject.Parse(combinedJsonText);

            var features = combinedJson["features"] as JArray;

            Dictionary<string, JObject> featuresByID = new Dictionary<string, JObject>();

            foreach(JObject feature in features)
            {
                var properties = feature["properties"];
                var originalid = (string)properties["originalid"];
                if(originalid == null)
                {
                    originalid = (string)feature["id"];
                }
                if (!featuresByID.ContainsKey(originalid))
                {
                    feature["id"] = originalid;
                    featuresByID.Add(originalid, feature);
                } else
                {
                    var dictFeature = featuresByID[originalid];
                    if((string)dictFeature["geometry"]["type"] != "MultiPolygon")
                    {
                        var nestedArray = new JArray();
                        nestedArray.Add(dictFeature["geometry"]["coordinates"]);
                        dictFeature["geometry"]["coordinates"] = nestedArray;
                        dictFeature["geometry"]["type"] = "MultiPolygon";
                    }
                    var coods = dictFeature["geometry"]["coordinates"] as JArray;
                    var newCoordinatesToAdd = feature["geometry"]["coordinates"];
                    coods.Add(newCoordinatesToAdd);
                }
            }

            foreach(var fkv in featuresByID)
            {
                var fileJson = new JObject();
                fileJson["type"] = "FeatureCollection";

                var featuresArray = new JArray();
                featuresArray.Add(fkv.Value);

                fileJson["features"] = featuresArray;

                var outputFilePath = (string)fkv.Value["properties"]["filename"];
                File.WriteAllText(outputFilePath, fileJson.ToString());
            }
            return 0;

        }
    }
}
