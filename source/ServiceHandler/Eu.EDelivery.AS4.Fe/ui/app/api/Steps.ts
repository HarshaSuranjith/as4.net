/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';
import { Step } from './Step'

export class Steps {
	decorator: string;
	step: Step[];

	static getForm(formBuilder: FormBuilder, current: Steps): FormGroup {
		return formBuilder.group({
				decorator: [current && current.decorator],
				step: formBuilder.array((current && current.step) === null ? [] : current.step.map(item => Step.getForm(formBuilder, item))),
		});
	}
}
