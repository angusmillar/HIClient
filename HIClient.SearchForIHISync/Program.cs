using System;

namespace HIClient.SearchForIHISync
{
  class Program
  {
    static void Main(string[] args)
    {
      //######################################################################
      //# Set up for IHI Search by Medicare Number, DVA Number or IHI Number #
      //######################################################################
      Console.WriteLine($"IHI Search by Medicare Number, DVA Number or IHI Number");
      // Set up client product information (PCIN)
      // Values below should be provided by Medicare
      var product = new nehta.mcaR3.ConsumerSearchIHI.ProductType()
      {
        platform = "Windows 8",     // Can be any value
        productName = "HIAgent",    // Provided by Medicare
        productVersion = "1.0.0",   // Provided by Medicare
        vendor = new nehta.mcaR3.ConsumerSearchIHI.QualifiedId()
        {
          id = "NEHTA001",          // Provided by Medicare               
          qualifier = "http://ns.electronichealth.net.au/id/hi/vendorid/1.0" // Provided by Medicare
        }
      };

      // Set up user identifier details
      var user = new nehta.mcaR3.ConsumerSearchIHI.QualifiedId()
      {
        id = "TestUserId", // User ID internal to your system
        qualifier = "http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0" // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Set up the calling organisation HPI-O identifier if required. 
      // Only required if interacting as a Contracted Service Provider (CSP)
      // If used, supply this 'hpio' instance to the client below as the 4th parameter. 
      // This must be the HPI-O of the site you are transacting for as part of your Contracted Service Provider role. 
      // This example assumes the use of a Medicare site certificate rather than a Medicare CSP Certificate
      // and therefore supplies null as the 4th parameter on the client below.
      var hpio = new nehta.mcaR3.ConsumerSearchIHI.QualifiedId()
      {
        id = "[Calling organisation's HPI-O] ",                   // HPIO internal to your system
        qualifier = "http://<anything>/id/<anything>/hpio/1.0"    // Eg: http://ns.yourcompany.com.au/id/yoursoftware/userid/1.0
      };

      // Obtain the HI Service certificate by serial number from the windows local machine certificate personal store 
      System.Security.Cryptography.X509Certificates.X509Certificate2 tlsCert = Nehta.VendorLibrary.Common.X509CertificateUtil.GetCertificate(
          "0608d9",
          System.Security.Cryptography.X509Certificates.X509FindType.FindBySerialNumber,
          System.Security.Cryptography.X509Certificates.StoreName.My,
          System.Security.Cryptography.X509Certificates.StoreLocation.LocalMachine,
          true
          );

      //The same certificate is used for signing the SOAP request. 
      //This certificate will be different to TLS cert for some operations.
      System.Security.Cryptography.X509Certificates.X509Certificate2 signingCert = tlsCert;

      //Instantiate the client
      //HiServiceTestEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      //HiServiceProdEndpoint = "https://www3.medicareaustralia.gov.au/pcert/soap/services/";
      string HiServiceEndpoint = "https://www5.medicareaustralia.gov.au/cert/soap/services/";
      var client = new Nehta.VendorLibrary.HI.ConsumerSearchIHIClient(
          new Uri(HiServiceEndpoint),
          product,
          user,
          null,
          signingCert,
          tlsCert);

      //Set up the request with the patient's demographics and identifiers
      //Must have (Family, DOB, Sex, and one of [MedicareNumber with optional IRN] or [DVA Number] or [IHI NUmber]) 
      var request = new nehta.mcaR3.ConsumerSearchIHI.searchIHI();

      // ------------------------------------------------------------------------------
      // 1. Medicare Number Search (IRN is Optional but recommended)
      // ------------------------------------------------------------------------------
      request.medicareCardNumber = "5950472601";
      request.medicareIRN = "1";
      request.dateOfBirth = DateTime.Parse("20 Jan 1988");
      request.givenName = "Ion";
      request.familyName = "HORNER";
      request.sex = nehta.mcaR3.ConsumerSearchIHI.SexType.M;

      // ------------------------------------------------------------------------------
      // 2. Department of Veteran Affairs number (DVA Number)     
      // ------------------------------------------------------------------------------
      //request.dvaFileNumber = "TX123458";
      //request.dateOfBirth = DateTime.Parse("27 Oct 1972");
      //request.givenName = "Jade";
      //request.familyName = "HAMPTON";
      //request.sex = SexType.F;

      // ------------------------------------------------------------------------------
      // 3. IHI Number Search/Validation
      // ------------------------------------------------------------------------------
      //request.ihiNumber = "http://ns.electronichealth.net.au/id/hi/ihi/1.0/8003601240022579";      
      //request.dateOfBirth = DateTime.Parse("27 Mar 1986");
      //request.givenName = "Janet";
      //request.familyName = "COX";
      //request.sex = SexType.F;

      // ------------------------------------------------------------------------------
      // 4. Australian Unstructured Street Address Search for IHI
      // ------------------------------------------------------------------------------            
      //request.dateOfBirth = DateTime.Parse("5 Aug 1965");
      //request.givenName = "Marie";
      //request.familyName = "VOSLOO";
      //request.sex = nehta.mcaR3.ConsumerSearchIHI.SexType.F;
      //request.australianUnstructuredStreetAddress = new nehta.mcaR3.ConsumerSearchIHI.AustralianUnstructuredStreetAddressType()
      //{
      //  addressLineOne = "189 Henrietta Quay",
      //  suburb = "COLLIE BURN",
      //  postcode = "6225",
      //  state = nehta.mcaR3.ConsumerSearchIHI.StateType.WA
      //};

      //##############################################
      //# Invoke the request against the HI Service  #
      //##############################################
      try
      {
        nehta.mcaR3.ConsumerSearchIHI.searchIHIResponse ihiResponse;
        //A different client method must be called depending on the type of identifier provided.
        if (!string.IsNullOrWhiteSpace(request.medicareCardNumber))
        {
          Console.WriteLine($"Performing IHI search by Medicare Number.");
          ihiResponse = client.BasicMedicareSearch(request);
        }
        else if (!string.IsNullOrWhiteSpace(request.dvaFileNumber))
        {
          Console.WriteLine($"Performing IHI search by DVA Number.");
          ihiResponse = client.BasicDvaSearch(request);
        }
        else if (!string.IsNullOrWhiteSpace(request.ihiNumber))
        {
          Console.WriteLine($"Performing IHI search by IHI Number (a.k.a IHI validation).");
          ihiResponse = client.BasicSearch(request);
        }
        else if (request.australianUnstructuredStreetAddress != null)
        {
          ihiResponse = client.AustralianUnstructuredAddressSearch(request);
        }
        else
        {
          throw new Exception("No search type for IHI search request. Must have one of Medicare Number, DVA Number, IHI Number or Australian Unstructured Address.");
        }

        //Here is the raw Soap Request and Response
        var RequestSoap = client.SoapMessages.SoapRequest;
        var ResponseSoap = client.SoapMessages.SoapResponse;

        //######################
        //# Report the results #
        //######################
        Console.WriteLine("==================================================================");
        Console.WriteLine("=== HI Service Search Results ====================================");
        Console.WriteLine("==================================================================");
        Console.WriteLine($"Patient family name: {ihiResponse.searchIHIResult.familyName}");
        Console.WriteLine($"Patient given Name: {ihiResponse.searchIHIResult.givenName}");
        Console.WriteLine($"Patient DOB: {ihiResponse.searchIHIResult.dateOfBirth.ToString("dd-MMM-yyyy")}");
        Console.WriteLine($"Patient sex: {ihiResponse.searchIHIResult.sex.ToString()}");
        Console.WriteLine($"Patient IHI Number: {ihiResponse.searchIHIResult.ihiNumber}");
        Console.WriteLine($"IHI Record Status: {ihiResponse.searchIHIResult.ihiRecordStatus.ToString()}");
        Console.WriteLine($"IHI Status: {ihiResponse.searchIHIResult.ihiStatus.ToString()}");
        Console.WriteLine("==================================================================");
        Console.WriteLine("Hit any key to end.");
        Console.ReadKey();
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
          nehta.mcaR3.ConsumerSearchIHI.ServiceMessagesType error = fault.GetDetail<nehta.mcaR3.ConsumerSearchIHI.ServiceMessagesType>();
          // Look at error details in here
          if (error.serviceMessage.Length > 0)
          {
            returnError = error.serviceMessage[0].code + ": " + error.serviceMessage[0].reason;
            Console.WriteLine($"Service Message: {returnError}");
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
  }
}
