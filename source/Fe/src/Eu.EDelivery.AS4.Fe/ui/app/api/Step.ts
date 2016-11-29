/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';


export class Step {
	type: string;
	unDecorated: boolean;

	static FIELD_type: string = 'type';
	static FIELD_unDecorated: string = 'unDecorated';

	static getForm(formBuilder: FormBuilder, current: Step): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			unDecorated: [current && current.unDecorated],
		});
	}
}
