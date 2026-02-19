using UnityEngine;

public static class StaticCrystal
{
    public class Atom
    {
        public Vector3 relativePos;
        public Color colour;

        public Atom(Vector3 relativePos, Color colour)
        {
            this.relativePos = relativePos; // The relative position of atoms in each instance of the basis.
            this.colour = colour; // The colours of atoms in each instance of the basis.
        }
    }
}
