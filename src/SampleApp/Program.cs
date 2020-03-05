using Portable.Licensing;
using Portable.Licensing.Security.Cryptography;
using Portable.Licensing.Validation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace SampleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = @"C:\";
            string fileName = "Licence.lic";

            Console.WriteLine("Enter the application name:");
            string appName = "Test";

            Console.WriteLine(appName);

            Console.WriteLine("Do you wish to create a new License? (Y/N):");
            var createLicense = Console.ReadLine();

            if (createLicense.Equals("Y", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("Enter the customer company:");
                var company = "Test Company";
                Console.WriteLine(company);

                Console.WriteLine("Enter the customer full name:");
                var fullname = "Test Name";
                Console.WriteLine(fullname);

                Console.WriteLine("Enter the customer email:");
                var email = "test@test.com";
                Console.WriteLine(email);

                Console.WriteLine("Enter the validity days before expiry (number of days):");
                var days = 30;
                Console.WriteLine(days);

                Console.WriteLine("Enter the number of allowed uses of this license (numbers only):");
                var count = 1;
                Console.WriteLine(count);

                var license = License.New();
                license.LicensedTo(fullname, email);


                if (days > 0)
                {
                    license = license.ExpiresAt(DateTime.Now.AddDays(days));
                }

                var productFeatures = new Dictionary<string, string>
                {
                    {"Version", "1.0"}
                };

                string passphrase = GeneratePassPhraseForProduct();
                var keyPair = GenerateKeysForProduct(passphrase);

                string privateKey = keyPair.ToEncryptedPrivateKeyString(passphrase);
                string publicKey = keyPair.ToPublicKeyString();


                var finalLicense = license.WithUniqueIdentifier(Guid.NewGuid())
                                               .As(LicenseType.Standard)
                                               .WithMaximumUtilization(1)
                                               .WithProductFeatures(productFeatures)
                                               .CreateAndSignWithPrivateKey(privateKey, passphrase);


                Console.WriteLine("Private key: {0}", privateKey);
                Console.WriteLine("Public key: {0}", publicKey);

                System.IO.File.WriteAllText(path + string.Format("{0}-private-key.txt", appName), privateKey);
                System.IO.File.WriteAllText(path + string.Format("{0}-public-key.txt", appName), publicKey);

                File.WriteAllText(path + fileName, finalLicense.ToString(), Encoding.UTF8);

                Console.WriteLine("License file successfully created");

                Console.WriteLine("Do you wish to validate the licence? (Y/N)");
                var validate = Console.ReadLine();

                if (validate.Equals("Y", StringComparison.OrdinalIgnoreCase))
                {
                    using (XmlReader reader = XmlReader.Create(path + fileName))
                    {
                        License readLicence = License.Load(reader);

                        var validationFailures = readLicence.Validate()
                          .ExpirationDate()
                          .When(lic => lic.Type == LicenseType.Standard)
                          .And()
                          .Signature(publicKey)
                          .AssertValidLicense().ToList();

                        foreach (var failure in validationFailures)
                            Console.WriteLine(failure.GetType().Name + ": " + failure.Message + " - " + failure.HowToResolve);
                    }
                }
            }

            Console.WriteLine("Press any key to exit");

            Console.ReadLine();
        }

        public static KeyPair GenerateKeysForProduct(string PassPhrase)
        {
            if (string.IsNullOrWhiteSpace(PassPhrase))
            {
                throw new Exception("No passphrase was given. Cannot generate keys.");
            }

            var keyGenerator = Portable.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();

            return keyPair;
        }

        public static string GeneratePassPhraseForProduct()
        {
            return Guid.NewGuid().ToString();
        }
    }
}
