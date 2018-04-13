using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace combiner
{
    public class CombinerProcessor
    {
        // combine "..\..\..\..\countries\USA.geo.json"
        public static int Combine(Program.CombineOptions options)
        {
            var geojsonFiles = Directory.GetFiles(Environment.CurrentDirectory).Where(f => Path.GetFileName( f) != "combined.json" && !Regex.IsMatch(Path.GetFileName(f), @"combiner\.") && Path.GetExtension(f) == ".json");


            var jsons = geojsonFiles.Select(f => new { filepath = f, json = JObject.Parse(File.ReadAllText(f)) }).ToList();

            int id = 0;
            var jObject = new JObject();
            jObject["type"] = "FeatureCollection";
            JArray featuresArray = new JArray();
            foreach (var json in jsons)
            {
                var features = json.json.GetValue("features") as JArray;
                
                foreach (var feature in features) {
                    feature["properties"]["filename"] = json.filepath;

                    var geometryNode = feature["geometry"];
                    if ((string)geometryNode["type"] == "MultiPolygon")
                    {
                        
                        var coorArray = geometryNode["coordinates"] as JArray;

                        foreach(JArray subArray in coorArray)
                        {
                            var featureClone = JObject.Parse(feature.ToString());
                            featureClone["properties"]["originalid"] = (string)featureClone["id"];
                            featureClone["id"] = ((string)featureClone["id"] ?? "") + id;
                            
                            featureClone["geometry"]["type"] = "Polygon";
                            featureClone["geometry"]["coordinates"] = subArray;
                            featuresArray.Add(featureClone);
                            id++;
                        }

                    } else
                    {
                        feature["properties"]["originalid"] = id;
                        id++;
                        featuresArray.Add(feature);
                    }                    
                }                
            }

            foreach(var feature in featuresArray)
            {
                var firstArray = feature["geometry"]["coordinates"][0] as JArray;
                var firstItem = firstArray[0] as JArray;
                var lat = (double)firstItem[0];
                var lng = (double)firstItem[1];

                var lastItem = firstArray[firstArray.Count() - 1];
                if(!double.Equals((double)firstItem[0], (double)lastItem[0]) || !double.Equals((double)firstItem[1], (double)lastItem[1]))
                {
                    firstArray.Add(firstItem);
                } 


            }

            jObject["features"] = featuresArray;

            var combinedJson = jObject.ToString();

            File.WriteAllText("combined.json", combinedJson);

            return 0;
        }
    }
}
