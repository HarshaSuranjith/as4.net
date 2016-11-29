/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class SettingsDatabase {
	provider: string;
	connectionString: string;

	static FIELD_provider: string = 'provider';
	static FIELD_connectionString: string = 'connectionString';

	static getForm(formBuilder: FormBuilder, current: SettingsDatabase): FormGroup {
		return formBuilder.group({
			provider: [current && current.provider],
			connectionString: [current && current.connectionString],
		});
	}
}
