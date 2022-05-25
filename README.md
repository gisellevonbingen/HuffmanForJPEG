# Huffman Algorithm And Table Serialization for JPEG

Just study for my other repository [Giselle.Imaging](https://github.com/gisellevonbingen/Giselle.Imaging)

# References
- https://www.digicamsoft.com/itu/itu-t81-45.html
- https://sunshowers.tistory.com/63?category=344080

# Examples

> Input String : AAAAAAABBCCCDEEEEFFFFFFGHIIJ

```
Nodes By Element

    0x41 : 10
    0x42 : 0111
    0x43 : 010
    0x44 : 11001
    0x45 : 111
    0x46 : 00
    0x47 : 11000
    0x48 : 11011
    0x49 : 0110
    0x4A : 11010

Nodes By Table

   Code Length : 1
   Simbols :

   Code Length : 2
   Simbols : 0x46, 0x41

   Code Length : 3
   Simbols : 0x43, 0x45

   Code Length : 4
   Simbols : 0x49, 0x42

   Length : 5
   Simbols : 0x47, 0x44, 0x4A, 0x48

Bitstream Encode/Decode Result

    Original
        BitStream       : 10101010101010011101110100100101100111111111111100000000000011000110110110011011010
        ByteStream      : 4141414141414142424343434445454545464646464646474849494A
        String          : AAAAAAABBCCCDEEEEFFFFFFGHIIJ

    Encode
        BitStream       : 1010101010101001110111010010010110011111111111110000000000001100011011011001101101000000
        ByteStream      : AAA9DD259FFF000C6D9B40

    Compare Size
        Huffman         : 28 => 11 (60.71%)
        Deflate         : 28 => 21 (25.00%)

    Decode
        BitStream       : 101010101010100111011101001001011001111111111111000000000000110001101101100110110100000
        String          : AAAAAAABBCCCDEEEEFFFFFFGHIIJFF    
```
