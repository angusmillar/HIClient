using System;
using System.Linq;

namespace HIClient.CreateVerifiedIHI
{
  class Program
  {
    static void Main(string[] args)
    {
      //##################################
      //# Set up for Create Verified IHI #
      //##################################
      Console.WriteLine($"Create Verified IHI");
      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR40.CreateVerifiedIHI.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR40.CreateVerifiedIHI.QualifiedId()
        {
          id = "NEHTA001",        // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0"                          // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR40.CreateVerifiedIHI.QualifiedId()
      {
        id = "TestUserId", // User ID internal to your system
        qualifier = "http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0" // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Set up user identifier details
      //var hpio = new nehta.mcaR40.CreateVerifiedIHI.QualifiedId()
      //{
      //  id = "HPIO",                                              // HPIO internal to your system
      //  qualifier = "http://<anything>/id/<anything>/hpio/1.0"    // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      //};

      // Obtain the certificate by serial number
      System.Security.Cryptography.X509Certificates.X509Certificate2 tlsCert = Nehta.VendorLibrary.Common.X509CertificateUtil.GetCertificate(
          "0608d9",
          System.Security.Cryptography.X509Certificates.X509FindType.FindBySerialNumber,
          System.Security.Cryptography.X509Certificates.StoreName.My,
          System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
          true
          );

      // The same certificate is used for signing the request.
      // This certificate will be different to TLS cert for some operations.
      System.Security.Cryptography.X509Certificates.X509Certificate2 signingCert = tlsCert;

      // Instantiate the client
      //HiServiceTestEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      //HiServiceProdEndpoint = "https://www3.medicareaustralia.gov.au/pcert/soap/services/";
      string HiServiceEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      var client = new Nehta.VendorLibrary.HI.ConsumerCreateVerifiedIHIClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          null,
          signingCert,
          tlsCert);

      // Set up the request by providing the patient details
      var request = new nehta.mcaR40.CreateVerifiedIHI.createVerifiedIHI();

      //Ref: HI Service - Create Newborn IHI Guide Final v1.0.pdf, Section 2.3 Business Rules, (page 7)
      //The request for an IHI to be assigned to a newborn must be created within 14 days
      //from birth.This is the maximum timeframe and your organisation may choose to
      //implement a shorter time limit.
      request.dateOfBirth = DateTime.Now;
      request.dateOfBirthAccuracyIndicator = nehta.mcaR40.CreateVerifiedIHI.DateAccuracyIndicatorType.AAA;

      //Ref: HI Service - Create Newborn IHI Guide Final v1.0.pdf, Section 2.3 Business Rules, (page 7)
      //Demographic details of the baby must be unique. A verified IHI record will not be
      //created by the HI Service where the same details match exactly with another verified
      //or unverified IHI record.The HI Service will use the following fields prior to assigning
      //a verified IHI to mitigate the risk of creating duplicate IHI number and record:
      //  a.Family Name(Preferred Name)
      //  b.Given Name(Preferred Name)
      //  c.Sex
      //  d.Date of Birth
      //  e.Address
      string RandomNameString = RandomString(6);
      request.familyName = $"Millar{RandomNameString}";
      request.givenName = new[] { $"Angus{RandomNameString}" };
      request.sex = nehta.mcaR40.CreateVerifiedIHI.SexType.F;
      request.usage = nehta.mcaR40.CreateVerifiedIHI.IndividualNameUsageType.L;
      request.address = new nehta.mcaR40.CreateVerifiedIHI.AddressType();
      request.address.australianStreetAddress = new nehta.mcaR40.CreateVerifiedIHI.AustralianStreetAddressType();
      request.address.australianStreetAddress.streetNumber = "111";
      request.address.australianStreetAddress.streetName = "Princess Road";
      request.address.australianStreetAddress.streetType = nehta.mcaR40.CreateVerifiedIHI.StreetType.ST;
      request.address.australianStreetAddress.streetTypeSpecified = true;
      request.address.australianStreetAddress.suburb = "Doubleview";
      request.address.australianStreetAddress.postcode = "6018";
      request.address.australianStreetAddress.state = nehta.mcaR40.CreateVerifiedIHI.StateType.WA;
      request.address.purpose = nehta.mcaR40.CreateVerifiedIHI.AddressPurposeType.R;
      request.address.preferred = nehta.mcaR40.CreateVerifiedIHI.TrueFalseType.T;
      request.privacyNotification = true;

      try
      {
        Console.WriteLine("==================================================================");
        Console.WriteLine("=== HI Service Create Verified IHI ===============================");
        Console.WriteLine("==================================================================");
        Console.WriteLine($"");
        Console.WriteLine($"Attempt to Create IHI for: ");
        Console.WriteLine($"Name: {request.familyName}, {request.givenName}");
        Console.WriteLine($"DOB: {request.dateOfBirth.ToString("dd-MMM-yyyy")}");
        Console.WriteLine($"Sex: {request.sex.ToString()}");
        Console.WriteLine($"");
        Console.WriteLine($"Making Call, please wait!");
        nehta.mcaR40.CreateVerifiedIHI.createVerifiedIHIResponse ihiResponse = client.CreateVerifiedIhi(request);

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;

        Console.WriteLine($"");
        Console.WriteLine($"Response");
        Console.WriteLine($"Created IHI Number: {ihiResponse.createVerifiedIHIResult.ihiNumber}");
        Console.WriteLine($"Created IHI Status: {ihiResponse.createVerifiedIHIResult.ihiStatus.ToString()}");
        Console.WriteLine($"Created IHI Record Status: {ihiResponse.createVerifiedIHIResult.ihiRecordStatus.ToString()}");
      }
      catch (System.ServiceModel.FaultException fex)
      {
        Console.WriteLine("==================================================================");
        Console.WriteLine("=== FaultException ===============================================");
        Console.WriteLine("==================================================================");
        string returnError = "";
        System.ServiceModel.Channels.MessageFault fault = fex.CreateMessageFault();
        if (fault.HasDetail)
        {
          nehta.mcaR40.CreateVerifiedIHI.ServiceMessagesType error = fault.GetDetail<nehta.mcaR40.CreateVerifiedIHI.ServiceMessagesType>();
          // Look at error details in here
          if (error.serviceMessage.Length > 0)
          {
            returnError = error.serviceMessage[0].code + ": " + error.serviceMessage[0].reason;
            Console.WriteLine($"Service Message: {returnError}");
            Console.WriteLine($"");
          }
        }

        // If an error is encountered, client.LastSoapResponse often provides a more
        // detailed description of the error.
        string soapResponse = client.SoapMessages.SoapResponse;
        Console.WriteLine($"Soap Response: {soapResponse}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
      catch (Exception ex)
      {
        // If an error is encountered, client.LastSoapResponse often provides a more
        // detailed description of the error.
        string soapResponse = client.SoapMessages.SoapResponse;
        Console.WriteLine($"Exception: {ex.Message}");
        Console.WriteLine($"Soap Response: {soapResponse}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
      }
    }

    public static string RandomString(int length)
    {
      Random random = new Random();
      const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
      return new string(Enumerable.Repeat(chars, length)
        .Select(s => s[random.Next(s.Length)]).ToArray());
    }



  }
}
