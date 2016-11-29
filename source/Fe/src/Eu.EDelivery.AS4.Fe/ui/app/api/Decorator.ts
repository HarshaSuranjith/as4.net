/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

import { Steps } from "./Steps"

export class Decorator {
	type: string;
	steps: Steps;

	static FIELD_type: string = 'type';
	static FIELD_steps: string = 'steps';

	static getForm(formBuilder: FormBuilder, current: Decorator): FormGroup {
		return formBuilder.group({
			type: [current && current.type],
			steps: Steps.getForm(formBuilder, current && current.steps),
		});
	}
}
