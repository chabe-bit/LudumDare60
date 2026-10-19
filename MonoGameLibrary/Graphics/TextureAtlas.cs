using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace MonoGameLibrary.Graphics
{
    public class TextureAtlas
    {
        // Two key members for managing texture regions:
        private Dictionary<string, TextureRegion> _regions; // Tracks textured regions, retrieving regions by name
        private Dictionary<string, Animation> _animations;
        public Texture2D Texture { get; set; }

        
        public TextureAtlas()
        {
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        public TextureAtlas(Texture2D texture)
        {
            Texture = texture;
            _regions = new Dictionary<string, TextureRegion>();
            _animations = new Dictionary<string, Animation>();
        }

        // Methods for managing texture regions and creating atlases 

        // We are effectively taking cut outs from a texture of which we are calling regions,
        // storing them into a dictionary with an ID form of a string name. Refering to the string
        // name will find the region. 

        public void AddRegion(string name, int x, int y, int width, int height)
        {
            TextureRegion region = new TextureRegion(Texture, x, y, width, height);
            _regions.Add(name, region); // Add() is a Dictionary method that adds a new item into the dictionary 
        }
   
        // Fetch region from texture by name
        public TextureRegion GetRegion(string name)
        {
            return _regions[name];
        }
    

        public bool RemoveRegion(string name)
        {
            if (_regions[name] != null)
            {
                return _regions.Remove(name);
            }

            return false;
        }

        public void Clear()
        {
            _regions.Clear(); // Remove everything from the dictionary
        }
    
        public static TextureAtlas FromFile(ContentManager content, string filename)
        {
            TextureAtlas atlas = new TextureAtlas();

            // Find the pathway 
            string path = Path.Combine(content.RootDirectory, filename);
            
            // Invalid path? Return empty atlas, do not end program
            if (path == null)
            {
                Console.Error.WriteLine($"File: {filename} is not found.");
                return atlas;
            }

            // Open the file and stream it
            using (Stream stream = TitleContainer.OpenStream(path))
            {
                using (XmlReader xml_reader = XmlReader.Create(stream))
                {
                    XDocument doc = XDocument.Load(xml_reader);
                    XElement root = doc.Root;


                    string texture_path = root.Element("Texture").Value;
                    atlas.Texture = content.Load<Texture2D>(texture_path);

                    var regions = root.Element("Regions")?.Elements("Region");
                
                    if (regions != null)
                    {
                        foreach (var region in regions)
                        {
                            string name = region.Attribute("name")?.Value;

                            // int.Parse() turns string into int
                            int x = int.Parse(region.Attribute("x")?.Value ?? "0");
                            int y = int.Parse(region.Attribute("y")?.Value ?? "0");
                            int width = int.Parse(region.Attribute("width")?.Value ?? "0");
                            int height = int.Parse(region.Attribute("height")?.Value ?? "0");
                    
                            if (!string.IsNullOrEmpty(name))
                            {
                                atlas.AddRegion(name, x, y, width, height);
                            }
                        }
                    }

                    // Parse [Animations] for Animation and Frame
                    var animation_elems = root.Element("Animations")?.Elements("Animation");


                    if (animation_elems != null)
                    {
                        // for every animation element 
                        foreach (var animation_elem in animation_elems)
                        {
                            // Parse for name and delay from [Animation]
                            string name = animation_elem.Attribute("name")?.Value;
                            float delay_ms = float.Parse(animation_elem.Attribute("delay")?.Value ?? "0");
                            TimeSpan delay = TimeSpan.FromMilliseconds(delay_ms);

                            List<TextureRegion> frames = new List<TextureRegion>();

                            var frame_elems = animation_elem.Elements("Frame");

                            if (frame_elems != null)
                            {
                                foreach (var frame_elem in frame_elems)
                                {
                                    string region_name = frame_elem.Attribute("region").Value;
                                    TextureRegion region = atlas.GetRegion(region_name);
                                    frames.Add(region);
                                }
                            }

                            Animation animation = new Animation(frames, delay);
                            atlas.AddAnimation(name, animation);
                        }
                    }


                    return atlas;
                }
            }
        }

        // Sprite Methods 
        public Sprite CreateSprite(string region_name)
        {
            TextureRegion region = GetRegion(region_name);
            return new Sprite(region);
        }


        // Animation Methods
        public void AddAnimation(string name, Animation animation)
        {
            _animations.Add(name, animation);
        }

        public Animation GetAnimation(string name)
        {
            return _animations[name];
        }

        public bool RemoveAnimation(string name)
        {
            if (_animations[name] != null)
            { 
                return _animations.Remove(name);
            }

            return false;
        }

        public AnimatedSprite CreateAnimatedSprite(string name)
        {
            Animation animation = GetAnimation(name);
            return new AnimatedSprite(animation);
        }

        public bool TryGetAnimation(string name, out Animation anim)
        {
            return _animations.TryGetValue(name, out anim);
        }

    }
}
