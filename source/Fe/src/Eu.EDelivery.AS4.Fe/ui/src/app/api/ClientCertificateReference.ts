/* tslint:disable */
import { FormBuilder, FormGroup, FormArray, Validators, FormControl } from '@angular/forms';

export class ClientCertificateReference {
	clientCertificateFindType: number;
	clientCertificateFindValue: string;

	static FIELD_clientCertificateFindType: string = 'clientCertificateFindType';
	static FIELD_clientCertificateFindValue: string = 'clientCertificateFindValue';
}
