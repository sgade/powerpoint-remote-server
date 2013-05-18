using System;

namespace PowerPoint_Remote.Server
{
    public abstract class PairingCodeGenerator
    {
        /// <summary>
        /// All the chars that are shown
        /// </summary>
        private static readonly char[] chars = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
                                                 /* 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z',  */
                                                 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z'};
        /// <summary>
        /// Shared instance of random.
        /// </summary>
        private static Random random = null;

        /// <summary>
        /// Generates a random code. This is different each time.
        /// </summary>
        /// <returns>The new code.</returns>
        public static String GenerateCode()
        {
            PairingCodeGenerator.random = new Random();
            String code = "";

            for ( int i = 0; i < Constants.SERVER_PAIRINGCODELENGTH; i++ )
            {
                code += PairingCodeGenerator.GenerateChar();
            }

            return code;
        }
        /// <summary>
        /// Generates a random char.
        /// </summary>
        /// <returns>A new random char.</returns>
        private static char GenerateChar()
        {
            int index = PairingCodeGenerator.random.Next(0, PairingCodeGenerator.chars.Length);
            return PairingCodeGenerator.chars[index];
        }
    }
}
