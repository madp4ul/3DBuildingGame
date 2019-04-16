using _1st3DGame.Menus.GraphicsMenuOptions;
using _1st3DGame.Input;
using Newtonsoft.Json;

namespace _1st3DGame.IO
{
    public class IODataContainer
    {
        public GraphicsOptions GraphicsOptions;
        public Keybindings Keybindings;

        protected IODataContainer() { }

        public static IODataContainer Load(string path)
        {
            if (System.IO.File.Exists(path))
                return JsonConvert.DeserializeObject<IODataContainer>(System.IO.File.ReadAllText(path));
            else
                return new IODataContainer()
                {
                    GraphicsOptions = GraphicsOptions.Default,
                    Keybindings = Keybindings.Default
                };
        }

        public void Save(string path)
        {
            System.IO.File.WriteAllText(path, JsonConvert.SerializeObject(this, Formatting.Indented));
        }
    }
}
