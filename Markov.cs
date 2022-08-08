using MarkovSharp.TokenisationStrategies;

namespace Music {
    public class Markov {
        StringMarkov model;
        StreamWriter writer;
        String serverID;
        public Markov(String id) {
            model = new StringMarkov(1);
            model.EnsureUniqueWalk = true;
            
            serverID = id;
            if (File.Exists("models/" + serverID + ".txt")) {
                var lines = File.Open("models/" + serverID + ".txt", FileMode.Open);
                var reader = new StreamReader(lines);
                writer = new StreamWriter(lines);
                model.Learn(reader.ReadToEnd().Split("\n"));
            } else {
                writer = new StreamWriter("models/" + serverID + ".txt");
            }
        }

        public void Add(string s) {
            model.Learn(s.Trim());
            writer.WriteLine(s);
            writer.Flush();
        }

        public string Generate() {
            return model.Walk().First();
        }

        public string Generate(String start) {
            return model.Walk(seed: start).First();
        }
    }
}