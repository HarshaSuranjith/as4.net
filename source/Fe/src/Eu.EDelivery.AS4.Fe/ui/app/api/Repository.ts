/* tslint:disable */
import { FormBuilder, FormGroup } from '@angular/forms';

export class Repository {
    type: string;

    static FIELD_type: string = 'type';

    static getForm(formBuilder: FormBuilder, current: Repository): FormGroup {
        return formBuilder.group({
            type: [current && current.type]
        });
    }
}
