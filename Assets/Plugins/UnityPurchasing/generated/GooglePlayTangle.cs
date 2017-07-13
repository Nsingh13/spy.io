#if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
// WARNING: Do not modify! Generated file.

namespace UnityEngine.Purchasing.Security {
    public class GooglePlayTangle
    {
        private static byte[] data = System.Convert.FromBase64String("YTUDUF5KNlu3Sf3alY6BOacYO4fpU2BF+UIMLTqkYJ/CSE3cO3W9tK+vqGGl9H5INFuV2GnthQ2HCTMel/ithqwD295Ww030ed/OaDv3kY+6q+5ICB5196PBsrclinQj4/BneoQHCQY2hAcMBIQHBwbiztTJK/0EE7/a/3fRp59HSYkfCDwpGrRIRObePOMI189BlrPkVpNlIqpWb6udYPeZ6gNMuHyOZiVR0aNDN7mKdqvnpRZZ3PR2OEonsHm8m4OImAYBykmSkB9C44VbkjLV4aDwaeA02Pfj8ApcsiH6vKyY0RWVgnyAXnLbWgc+NoQHJDYLAA8sgE6A8QsHBwcDBgXFvW0+PFpEulfzLtLecUXkR56C5jEl0/DZhswHnQQFBwYH");
        private static int[] order = new int[] { 4,2,12,10,5,12,9,8,8,9,13,11,12,13,14 };
        private static int key = 6;

        public static byte[] Data() {
            return Obfuscator.DeObfuscate(data, order, key);
        }
    }
}
#endif
