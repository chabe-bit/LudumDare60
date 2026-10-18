using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using MonoGameLibrary.Graphics;


namespace LudumDare60
{
    [Flags]
    public enum Component: uint
    {
        None            = 0,
        Position        = 1 << 0,
        Move            = 1 << 1,
        Sprite          = 1 << 2,
        Input           = 1 << 3,
        Health          = 1 << 5,
        Enemy           = 1 << 6,
        Dead            = 1 << 7,
    }
    internal class AnimState
    {
        public RoboState state;
        public Direction dir;
        public int last_frame;
    }

    public class World
    {
        public const int max_entities = 256;

        // A marker for the next open slot
        public int open_slot;

        public Component[] _mask = new Component[max_entities];
        public Vector3[] _pos = new Vector3[max_entities];
        public Vector3[] _facing = new Vector3[max_entities];
        public Vector3[] _move = new Vector3[max_entities];
        public float[] _speed = new float[max_entities];
        public AnimatedSprite[] _sprite = new AnimatedSprite[max_entities];        
        public int[] _hp = new int[max_entities]; 

        readonly Stack<int> _free = new();
        readonly List<int> _destroy = new();
        
        public int Create(Component mask)
        {
            int ent;
            if (_free.Count > 0)
            {
                ent = _free.Pop();
            } 
            else if (open_slot < max_entities)
            {
                ent = open_slot++;
            }
            else
            {
                return -1;
            }

            _mask[ent] = mask;
            return ent;
        }

        // Here we're checking every bit of an entity other than its death state
        public bool Has(int ent, Component comp) => (_mask[ent] & (comp | Component.Dead)) == comp;

        public void Destroy(int ent)
        {
            // For the entity we are targeting, check either it exist
            // or already dead. If either are true, exit this function.
            if ((_mask[ent] == Component.None) ||
                (_mask[ent] & Component.Dead) != 0)
            {
                return;
            }

            // Turn on the flag that it's dead and queue it in, resolution to destroy in Flush 
            _mask[ent] |= Component.Dead;
            _destroy.Add(ent);
        }

        public void Flush()
        {
            foreach (int ent in _destroy)
            {
                // Vacate the spot the destroyed entity was sitting in! 
                _mask[ent] = Component.None;
                _sprite[ent] = null;
                _move[ent] = Vector3.Zero;
                _hp[ent] = 0;
                _free.Push(ent);
            }

            _destroy.Clear();
        }
        public int Count(Component comp)
        {
            int n = 0;
            for (int ent = 0; ent < open_slot; ent++)
            {
                if (Has(ent, comp))
                {
                    n++;
                }
            }
            return n;
        }
    }
}
