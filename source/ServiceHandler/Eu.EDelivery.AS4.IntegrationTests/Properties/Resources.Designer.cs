﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Eu.EDelivery.AS4.IntegrationTests.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("Eu.EDelivery.AS4.IntegrationTests.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\errors.
        /// </summary>
        internal static string as4_component_errors_path {
            get {
                return ResourceManager.GetString("as4_component_errors_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\exceptions.
        /// </summary>
        internal static string as4_component_exceptions_path {
            get {
                return ResourceManager.GetString("as4_component_exceptions_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\receipts.
        /// </summary>
        internal static string as4_component_receipts_path {
            get {
                return ResourceManager.GetString("as4_component_receipts_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--=-WoWSZIFF06iwFV8PHCZ0dg==
        ///Content-Type: application/soap+xml; charset=utf-8
        ///
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&lt;s12:Envelope xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot;&gt;&lt;s12:Header&gt;&lt;wsse:Security xmlns:wsse=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd&quot;&gt;&lt;BinarySecurityToken EncodingType=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary&quot; ValueType=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x50 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4_soap_wrong_encrypted_message {
            get {
                return ResourceManager.GetString("as4_soap_wrong_encrypted_message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;s12:Envelope xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot;&gt;
        ///  &lt;s12:Header&gt;
        ///    &lt;wsse:Security xmlns:wsse=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd&quot;&gt;
        ///      &lt;Signature xmlns=&quot;http://www.w3.org/2000/09/xmldsig#&quot;&gt;
        ///        &lt;SignedInfo&gt;
        ///          &lt;CanonicalizationMethod Algorithm=&quot;http://www.w3.org/2001/10/xml-exc-c14n#&quot; /&gt;
        ///          &lt;SignatureMethod Algorithm=&quot;http://www.w3.org/2001/04/xmldsig-more#rsa-sha256&quot; /&gt;
        ///      [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4_soap_wrong_signed_callback_message {
            get {
                return ResourceManager.GetString("as4_soap_wrong_signed_callback_message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&lt;s12:Envelope xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot;&gt;&lt;s12:Header&gt;&lt;Security xmlns=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd&quot;&gt;&lt;BinarySecurityToken EncodingType=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-soap-message-security-1.0#Base64Binary&quot; ValueType=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-x509-token-profile-1.0#X509v3&quot; p4:Id=&quot;cert-c4488537-0e5a-4360-b7f0-a14ba48d52e0&quot; xmlns:p4=&quot;http:// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4_soap_wrong_signed_message {
            get {
                return ResourceManager.GetString("as4_soap_wrong_signed_message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--=-M9awlqbs/xWAPxlvpSWrAg==
        ///Content-Type: application/soap+xml; charset=utf-8
        ///
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&lt;s12:Envelope xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot;&gt;&lt;s12:Header&gt;&lt;eb:Messaging xmlns:wsu=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd&quot; xmlns:wsse=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd&quot; xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot; wsu:Id=&quot;header-fcd4d20d-3842-479a-bcaf-91a8a6d97275&quot; xm [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4message_external_payloads {
            get {
                return ResourceManager.GetString("as4message_external_payloads", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--MIMEBoundary_58227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295
        ///Content-Type: application/soap+xml; charset=UTF-8
        ///Content-Transfer-Encoding: binary
        ///Content-ID: &lt;0.68227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295@apache.org&gt;
        ///
        ///&lt;?xml version=&apos;1.0&apos; encoding=&apos;UTF-8&apos;?&gt;&lt;soapenv:Envelope xmlns:soapenv=&quot;http://www.w3.org/2003/05/soap-envelope&quot; xmlns:xsd=&quot;http://www.w3.org/1999/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/1999/XMLSchema-instance/&quot; xmlns:eb3=&quot;http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/20 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4message_incorect_compressed {
            get {
                return ResourceManager.GetString("as4message_incorect_compressed", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--MIMEBoundary_58227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295
        ///Content-Type: application/soap+xml; charset=UTF-8
        ///Content-Transfer-Encoding: binary
        ///Content-ID: &lt;0.68227ff3e3fc7f2a7373840dd22c75172d4362e9ce55d295@apache.org&gt;
        ///
        ///&lt;?xml version=&apos;1.0&apos; encoding=&apos;UTF-8&apos;?&gt;&lt;soapenv:Envelope xmlns:soapenv=&quot;http://www.w3.org/2003/05/soap-envelope&quot; xmlns:xsd=&quot;http://www.w3.org/1999/XMLSchema&quot; xmlns:xsi=&quot;http://www.w3.org/1999/XMLSchema-instance/&quot; xmlns:eb3=&quot;http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/20 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string as4message_missing_mime_property {
            get {
                return ResourceManager.GetString("as4message_missing_mime_property", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to 
        ///--=-M9awlqbs/xWAPxlvpSWrAg==
        ///Content-Type: application/soap+xml; charset=utf-8
        ///
        ///&lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;&lt;s12:Envelope xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot;&gt;&lt;s12:Header&gt;&lt;eb:Messaging xmlns:wsu=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-utility-1.0.xsd&quot; xmlns:wsse=&quot;http://docs.oasis-open.org/wss/2004/01/oasis-200401-wss-wssecurity-secext-1.0.xsd&quot; xmlns:s12=&quot;http://www.w3.org/2003/05/soap-envelope&quot; wsu:Id=&quot;header-fcd4d20d-3842-479a-bcaf-91a8a6d97275&quot; xm [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string duplicated_as4message {
            get {
                return ResourceManager.GetString("duplicated_as4message", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Drawing.Bitmap.
        /// </summary>
        internal static System.Drawing.Bitmap earth {
            get {
                object obj = ResourceManager.GetObject("earth", resourceCulture);
                return ((System.Drawing.Bitmap)(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-A\data\msg_in.
        /// </summary>
        internal static string holodeck_A_input_path {
            get {
                return ResourceManager.GetString("holodeck_A_input_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-A\data\msg_out.
        /// </summary>
        internal static string holodeck_A_output_path {
            get {
                return ResourceManager.GetString("holodeck_A_output_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-A\conf\pmodes.
        /// </summary>
        internal static string holodeck_A_pmodes {
            get {
                return ResourceManager.GetString("holodeck_A_pmodes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-B\data\msg_in.
        /// </summary>
        internal static string holodeck_B_input_path {
            get {
                return ResourceManager.GetString("holodeck_B_input_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-B\data\msg_out.
        /// </summary>
        internal static string holodeck_B_output_path {
            get {
                return ResourceManager.GetString("holodeck_B_output_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-B\conf\pmodes.
        /// </summary>
        internal static string holodeck_B_pmodes {
            get {
                return ResourceManager.GetString("holodeck_B_pmodes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-A\data\msg_out\payloads\dandelion.jpg.
        /// </summary>
        internal static string holodeck_payload_path {
            get {
                return ResourceManager.GetString("holodeck_payload_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \config\integrationtest-pmodes\holodeck-pmodes.
        /// </summary>
        internal static string holodeck_test_pmodes {
            get {
                return ResourceManager.GetString("holodeck_test_pmodes", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to C:\Program Files\Java\holodeck\holodeck-b2b-A\data\msg_out\payloads\simple_document.xml.
        /// </summary>
        internal static string holodeck_xml_payload_path {
            get {
                return ResourceManager.GetString("holodeck_xml_payload_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\errors.
        /// </summary>
        internal static string notify_error_path {
            get {
                return ResourceManager.GetString("notify_error_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;PMode xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot; xmlns=&quot;eu:edelivery:as4:pmode&quot;&gt;
        ///  &lt;Id&gt;push-sample&lt;/Id&gt;
        ///  &lt;AllowOverride&gt;false&lt;/AllowOverride&gt;
        ///  &lt;Mep&gt;OneWay&lt;/Mep&gt;
        ///  &lt;MepBinding&gt;Push&lt;/MepBinding&gt;
        ///  &lt;PushConfiguration&gt;
        ///    &lt;Protocol&gt;
        ///      &lt;UseChunking&gt;false&lt;/UseChunking&gt;
        ///      &lt;UseHttpCompression&gt;false&lt;/UseHttpCompression&gt;
        ///    &lt;/Protocol&gt;
        ///    &lt;TlsConfiguration&gt;
        ///      &lt;IsEnabled&gt;false&lt;/IsEnabled&gt;
        ///      [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string pmodexml {
            get {
                return ResourceManager.GetString("pmodexml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\in.
        /// </summary>
        internal static string submit_input_path {
            get {
                return ResourceManager.GetString("submit_input_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages.
        /// </summary>
        internal static string submit_messages_path {
            get {
                return ResourceManager.GetString("submit_messages_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\out.
        /// </summary>
        internal static string submit_output_path {
            get {
                return ResourceManager.GetString("submit_output_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;SubmitMessage xmlns=&quot;urn:cef:edelivery:eu:as4:messages&quot;&gt;
        ///  &lt;Collaboration&gt;
        ///    &lt;AgreementRef&gt;
        ///      &lt;PModeId&gt;push-sample&lt;/PModeId&gt;
        ///      &lt;Value&gt;http://agreements.holodeckb2b.org/examples/agreement0&lt;/Value&gt;
        ///    &lt;/AgreementRef&gt;
        ///    &lt;ConversationId&gt;eu:edelivery:as4:sampleconversation&lt;/ConversationId&gt;
        ///  &lt;/Collaboration&gt;
        ///
        ///  &lt;MessageProperties&gt;
        ///    &lt;MessageProperty&gt;
        ///      &lt;Name&gt;Test&lt;/Name&gt;
        ///      &lt;ExceptionType&gt;Test&lt;/ExceptionType&gt;
        ///      &lt;Value&gt;Test&lt;/Value&gt;
        ///    &lt;/MessageProper [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string submitmessage_multiple_payloads_xml {
            get {
                return ResourceManager.GetString("submitmessage_multiple_payloads_xml", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\attachments\xml-sample.xml.
        /// </summary>
        internal static string submitmessage_second_payload_path {
            get {
                return ResourceManager.GetString("submitmessage_second_payload_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to \messages\attachments\earth.jpg.
        /// </summary>
        internal static string submitmessage_single_payload_path {
            get {
                return ResourceManager.GetString("submitmessage_single_payload_path", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot;?&gt;
        ///&lt;SubmitMessage xmlns=&quot;urn:cef:edelivery:eu:as4:messages&quot;&gt;
        ///  &lt;Collaboration&gt;
        ///    &lt;AgreementRef&gt;
        ///      &lt;PModeId&gt;push-sample&lt;/PModeId&gt;
        ///      &lt;Value&gt;http://agreements.holodeckb2b.org/examples/agreement0&lt;/Value&gt;
        ///    &lt;/AgreementRef&gt;
        ///    &lt;ConversationId&gt;eu:edelivery:as4:sampleconversation&lt;/ConversationId&gt;
        ///  &lt;/Collaboration&gt;
        ///
        ///  &lt;MessageProperties&gt;
        ///    &lt;MessageProperty&gt;
        ///      &lt;Name&gt;Test&lt;/Name&gt;
        ///      &lt;ExceptionType&gt;Test&lt;/ExceptionType&gt;
        ///      &lt;Value&gt;Test&lt;/Value&gt;
        ///    &lt;/MessageProper [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string submitmessage_single_payload_xml {
            get {
                return ResourceManager.GetString("submitmessage_single_payload_xml", resourceCulture);
            }
        }
    }
}
