LibTessDotNet source provenance

Repository https://github.com/speps/LibTessDotNet

Revision bdc0b0f94904dd94c3edfb2c25aa358695276e09

The eight C# files come from LibTessDotNet/Sources at this revision. Integration changes prepend the DOUBLE and UNITY_EDITOR directives, enclose each file in the editor conditional, and replace the namespace LibTessDotNet with AeternumGames.ShapeEditor.LibTessDotNet. Algorithms, original comments, and copyright notices retain the upstream text.

The upstream UTF-8 byte-order mark is removed before adding the conditional wrapper.

DOUBLE selects the library's existing double-precision implementation. The namespace prefix isolates its public types from other Unity packages. The editor conditional follows ShapeEditor's other decomposition sources.

The full license is in Licenses/LibTessDotNet.txt. LibTessDotNet uses SGI Free Software License B version 2.0.
