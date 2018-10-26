## Sending Processing Mode

This contract describes all the properties available in the Sending PMode. The required data fields are marked as mandatory; default values are provided. Some values of the Sending PMode can be overridden by a SubmitMessage. This definition is available as XSD.

<table>
    <tbody>
        <tr>
            <th align="left"><b>Sending PMode</b></th>
            <th align="center">*</th>
            <th align="left">Description</b></u></th>
        </tr>
        <tr>
            <td><b>Id</b></td>
            <td align="center">M</td>
            <td>PMode Unique Id</td>
        </tr>
        <tr>
            <td><b>AllowOverride</b></td>
            <td align="center">O</td>
            <td>
                <div style="width:550px;">Boolean indication whether a SubmitMessage may override already configured values within the sending PMode. <br/> <i>Default:</i> false</div>
            </td>
        </tr>
        <tr>
            <td><b>MEP</b></td>
            <td align="center">M</td>
            <td>
                Message Exchange Pattern
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>OneWay</li>
                    <li>TwoWay</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td><b>MEPBinding</b></td>
            <td align="center">M</td>
            <td>
                Message Exchange Pattern Binding
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>push</li>
                    <li>pull</li>
                </ul>
            </td>
        </tr>
        <tr>
            <td>
                <b>PushConfiguration</b><br/>
                &nbsp;&nbsp;<i><u>Protocol</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;URL<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseChunking<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;UseHTTPCompression<br/>
                &nbsp;&nbsp;<i><u>TLSConfiguration</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TLSVersion<br/>
                <br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<br/><i><u>ClientCertificateReference</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ClientCertificateFindType<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;ClientCertificateFindValue
            </td>
            <td align="center">
                O<br/>
                M<br/>
                M<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                <br/><br/><br/><br/>
                M<br/>
                M<br/>
            </td>
            <td>
                Element must be present when MEPBinding is set to <i>push</i><br/>
                &nbsp;URL of the receiving MSH<br/>
                <i>Default: </i>false (true &gt; not implemented)<br/>
                <i>Default: </i>false (true &gt; not implemented)<br/><br/>
                <i>Default:</i> false<br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>SSL 3.0</li>
                    <li>TLS 1.0</li>
                    <li>TLS 1.1</li>
                    <li>TLS 1.2</li>
                </ul>
                Information on how to retrieve the SSL certificate<br/><br/><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>DynamicDiscovery</b><br/>
                &nbsp;&nbsp;SmpProfile<br/><br/>
                &nbsp;&nbsp;<i>Settings</i><br/>
                &nbsp;&nbsp;&nbsp;<i>Setting</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;Key<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/><br/>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
            </td>
            <td>
                This element is only present when SMP/SML is required<br/>
                The FQN of the class that implements the IDynamicDiscoveryProfile interface that must be used. If this is not defined, the internal implementation must be used by default.<br/>
                Custom settings to configure the IDynamicDiscoveryProfile.
                <br/><br/><br/><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Reliability</b><br/>
                &nbsp;&nbsp; <i><u>ReceptionAwareness</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;RetryCount<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;RetryInterval<br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
                <br/><br/>
                <i>Default:</i> false<br/>
                <i>Default:</i> 5<br/>
                <i>Default:</i> 00:01:00 (HH:mm:ss)<br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>ReceiptHandling</b><br/>
                &nbsp;&nbsp;VerifyNRR<br/><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;Reliability<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>IsEnabled</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryCount</i><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryInterval</i><br/><br/>
            </td>
            <td>
                O<br/>
                O<br/><br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                O<br/>
                M<br/>
                O<br/><br/>
                O<br/><br/>
            </td>
            <td>
                <br/>
                Indicates if Non-Repudiation of Receipt must be verified.<br/><i>Default:</i> false<br/>
                <i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/><br/>
                Indicate wheter or not the deliver operation should be retried on failure<br/>
                Amount of retry cycles the deliver operation should be retried on failure<br/>
                <i>Default: 4</i><br/>
                Interval between each retry cycle during the Delivery<br/>
                <i>Default: 0:00:01:00 (1 min)</i>
            </td>
        </tr>
        <tr>
            <td>
                <b>ErrorHandling</b><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameters</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;Reliability<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>IsEnabled</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryCount</i><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryInterval</i><br/><br/>
            </td>
            <td>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                O<br/>
                M<br/>
                O<br/><br/>
                O<br/><br/>
            </td>
            <td>
                <br/><i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/><br/>
                Indicate wheter or not the deliver operation should be retried on failure<br/>
                Amount of retry cycles the deliver operation should be retried on failure<br/>
                <i>Default: 4</i><br/>
                Interval between each retry cycle during the Delivery<br/>
                <i>Default: 0:00:01:00 (1 min)</i>
            </td>
        </tr>
        <tr>
            <td>
                <b>ExceptionHandling</b><br/>
                &nbsp;&nbsp;NotifyMessageProducer<br/>
                &nbsp;&nbsp;NotifyMethod<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Type</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i><u>Parameters</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>Parameter</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp; &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;Reliability<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>IsEnabled</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryCount</i><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>RetryInterval</i><br/><br/>
            </td>
            <td>
                O<br/>
                O<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                M<br/>
                O<br/>
                M<br/>
                O<br/><br/>
                O<br/><br/>
            </td>
            <td>
                <br/><i>Default:</i> false<br/><br/>
                Type of the Notify Agent<br/>
                Required parameters for the specified agent<br/><br/>
                Name of the parameter<br/>
                Value of the parameter<br/><br/>
                Indicate wheter or not the deliver operation should be retried on failure<br/>
                Amount of retry cycles the deliver operation should be retried on failure<br/>
                <i>Default: 4</i><br/>
                Interval between each retry cycle during the Delivery<br/>
                <i>Default: 0:00:01:00 (1 min)</i>
            </td>
        </tr>
        <tr>
            <td>
                <b>Security</b><br/>
                <b>&nbsp;&nbsp;</b><i><u>Signing</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindCriteria<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindType
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindValue<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;KeyReferenceMethod
                <br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Algorithm<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;HashFunction<br/>
                &nbsp;&nbsp;<i>SigningVerification</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Signature<br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;AllowUnknownRootCertificate
                &nbsp;&nbsp;<i><u>Encryption</u></i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;IsEnabled<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PublicKeyCertificate<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Certificate<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindCriteria<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindType
                <br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;CertificateFindValue
                <br/>
                Algorithm<br/><br/><br/><br/><br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;AlgorithmKeySize<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;KeyTransport<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;TransportAlgorithm<br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;DigestAlgorithm<br/><br/><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;MgfAlgorithm<br/>
                <br/><br/><br/>
                <br/><br/><br/><br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                M<br/>
                M<br/><br/><br/><br/>
                M<br/>
                O<br/>
                O<br/>
                M<br/><br/><br/><br/><br/><br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                M<br/>
                O<br/>
                M<br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/><br/>
                M<br/>
                M<br/><br/><br/><br/><br/><br/><br/>
                O<br/>
                O<br/>
                O<br/><br/><br/>
                O<br/><br/><br/>
                O<br/><br/>
                <br/><br/><br/>
                <br/><br/><br/>
            </td>
            <td>
                <br/><br/>
                <i>Default:</i> false<br/>
                <i>&nbsp;</i><br/>
                <i>Enumeration:</i><br/>
                <ul style="margin:0;">
                    <li>FindByThumbprint</li>
                    <li>FindBySubjectName</li>
                    <li>FindBySubjectDistinguishedName</li>
                    <li>FindByIssuerName</li>
                    <li>FindByIssuerDistinguishedName</li>
                    <li>FindBySerialNumber</li>
                    <li>FindByTimeValid</li>
                    <li>FindByTimeNotValid</li>
                    <li>FindByTimeNotYetValid</li>
                    <li>FindByTimeExpired</li>
                    <li>FindByTemplateName</li>
                    <li>FindByApplicationPolicy</li>
                    <li>FindByCertificatePolicy</li>
                    <li>FindByExtension</li>
                    <li>FindByKeyUsage</li>
                    <li>FindBySubjectKeyIdentifier</li>
                </ul><br/><br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>BSTReference <i>(default)</i></li>
                    <li>IssuerSerial</li>
                    <li>KeyIdentifier</li>
                </ul>
                <br/><br/><br/>
                <i>Enumeration</i>
                <ul style="margin:0;">
                    <li>Allowed (<i>default)</i></li>
                    <li>Not allowed</li>
                    <li>Required</li>
                    <li>Ignored</li>
                </ul>
                <br/>
                <div style="width:550px;">Indicates whether certificates with an unknown root authority are trusted. (Default <i>false</i>)</div><br/>               <i>Enumeration:</i>
                <i>Default</i>: false<br/>
                PublicKeyCertificate or CertificateFindCriteria must be specified<br/>
                Base64 representation of the certificate that must be used<br/>
                PublicKeyCertificate or CertificateFindCriteria must be specified<br/>
                <i>Enumeration:</i>
                <ul style="margin:0;">
                    <li>FindByThumbprint</li>
                    <li>FindBySubjectName</li>
                    <li>FindBySubjectDistinguishedName</li>
                    <li>FindByIssuerName</li>
                    <li>FindByIssuerDistinguishedName</li>
                    <li>FindBySerialNumber</li>
                    <li>FindByTimeValid</li>
                    <li>FindByTimeNotValid</li>
                    <li>FindByTimeNotYetValid</li>
                    <li>FindByTimeExpired</li>
                    <li>FindByTemplateName</li>
                    <li>FindByApplicationPolicy</li>
                    <li>FindByCertificatePolicy</li>
                    <li>FindByExtension</li>
                    <li>FindByKeyUsage</li>
                    <li>FindBySubjectKeyIdentifier</li>
                </ul>
                <i><br/>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#aes128-gcm</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#des-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#tripledes-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes128-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes192-cbc</a></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#aes256-cbc</a></li>
                </ul>
                <i>Supported values:</i> 128, 192, 256.&nbsp; Default is 128<br/><br/>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#rsa-oaep</a> <i>(default)</i></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#rsa-oaep-mgf1p</a></li>
                </ul>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2000/09/xmldsig#sha1</a><i> </i></li>
                    <li><a>http://www.w3.org/2001/04/xmlenc#sha256</a> <i>(default)</i></li>
                </ul>
                <i>Supported values:</i>
                <ul style="margin:0;">
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha1</a> <i>(default)</i></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha224</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha256</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha384</a></li>
                    <li><a>http://www.w3.org/2009/xmlenc11#mgf1sha512</a></li>
                </ul>
                Note that the Mgf Algorithm cannot be specified when the TransportAlgorithm is not set to <a>http://www.w3.org/2009/xmlenc11#rsa-oaep</a><br/>
            </td>
        </tr>
        <tr>
            <td>
                <b>Message Packaging</b><br/>
                &nbsp;&nbsp;Mpc<br/>
                &nbsp;&nbsp;UseAS4Compression<br/>
                &nbsp;&nbsp;IsMultiHop<br/>
                &nbsp;&nbsp;IncludePModeId<br/>
                &nbsp;&nbsp;<u>PartyInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>FromParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>PartyIds</i>
                <br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;<i>ToParty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>PartyIds</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;PartyId</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Id<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Role<br/>
                &nbsp;&nbsp;<u>CollaborationInfo</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>AgreementRef</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>Service</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; Action<br/>
                &nbsp;&nbsp;<u>MessageProperties</u><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp; <i>MessageProperty</i><br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Name<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Type<br/>
                &nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;Value<br/>
            </td>
            <td>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
                O<br/>
            </td>
            <td>
                <br/>
                <i>Default: </i><a>http://docs.oasis-open.org/ebxml-msg/ebms/v3.0/ns/core/200704/defaultMPC</a><br/>
                <i>Default:</i> false</i><br/>
                <i>Default:</i> false</i><br/>
                <i>Default:</i> false</i><br/>
                <br/><br/><br/><br/>
                Id of the sending party<br/>
                Type of Id of the sending party<br/>
                Role of the sending party<br/>
                <br/><br/><br/>
                Id of the receiving party<br/>
                Type of Id of the receiving party<br/>
                Role of the receiving party<br/><br/>
                Information about the partner agreement<br/>
                <br/>
                Type of the agreement reference<br/>
                <br/>
                The name of the service that is consumed<br/>
                Type of the service<br/>
                The service operation that is consumed<br/>
                <br/><br/>
                Name of the message property<br/>
                Type of the message property<br/>
                Value of the message property<br/>
            </td>
        </tr>
    </tbody>
</table>

(\*): M = Mandatory | O = Optional | R = Recommended
