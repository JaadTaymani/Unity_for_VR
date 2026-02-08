using System;
using System.Collections.Generic;

// A class used to store saved structure data in its simplest form to be converted to or from JSON.
[Serializable]
public class SaveFile
{
    public int Size;

    // The three basis vectors:
    public float[] A1;
    public float[] A2;
    public float[] A3;

    public List<SaveAtom> BasisAtoms;
}

// A class used to store saved StaticCrystal.Atom type object data in its simplest form to be converted to or from JSON.
[Serializable]
public class SaveAtom
{
    public float[] RelativePos;
    public float[] Colour;
}
