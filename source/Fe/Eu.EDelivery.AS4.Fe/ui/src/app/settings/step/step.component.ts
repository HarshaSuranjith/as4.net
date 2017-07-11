import { FormGroup, FormArray, FormBuilder, AbstractControl } from '@angular/forms';
import { NgZone, Component, Input, OnDestroy, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';

import { DialogService } from './../../common/dialog.service';
import { RuntimeService } from '../runtime.service';
import { ItemType } from './../../api/ItemType';
import { RuntimeStore } from '../runtime.store';
import { SettingForm } from './../../api/SettingForm';
import { StepForm } from './../../api/StepForm';

@Component({
    selector: 'as4-step-settings',
    template: `
        <div [formGroup]="group">
            <p><button as4-auth type="button" [disabled]="disabled" class="btn btn-flat" (click)="addStep()"><i class="fa fa-plus"></i></button></p>
            <div [sortablejs]="group" [sortablejsOptions]="{ handle: '.grippy', onEnd: itemMoved }">
                <div *ngFor="let step of group.controls; let i = index" [formGroupName]="i">
                    <div class="step-row">
                        <div class="item"><span class="grippy"></span></div>
                        <div class="item"><button as4-auth [disabled]="disabled" type="button" class="btn btn-flat" (click)="removeStep(i)"><i class="fa fa-trash-o"></i></button></div>
                        <div class="item">
                            <select class="form-control" formControlName="type" (change)="stepChanged(step, selectedStep.value)" #selectedStep>    
                                <option *ngFor="let step of steps" [value]="step.technicalName">{{step.name}}</option>
                            </select>
                            <div class="settings">
                                <as4-runtime-settings showTitle="false" [form]="step" [types]="steps" [itemType]="step.get('type').value"></as4-runtime-settings>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    `,
    styles: [
        `
            .settings {
                padding-top: 10px;
            }
            table tr td {
                border: 0;
            }
            .step-row {
                display: flex;
            }
            .step-row > .item:last-child {
                flex: 1 1 auto;
            }
        `
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class StepSettingsComponent implements OnDestroy {
    @Input() public group: FormArray;
    @Input() public disabled: boolean = true;
    public steps: ItemType[];
    private _runtimeStoreSubscription: Subscription;
    constructor(private formBuilder: FormBuilder, private runtimeStore: RuntimeStore, private dialogService: DialogService, private _changeDetectorRef: ChangeDetectorRef) {
        this._runtimeStoreSubscription = this.runtimeStore
            .changes
            .filter((result) => result != null)
            .subscribe((result) => {
                this.steps = result.steps;
            });
    }
    public itemMoved = () => {
        this.group.markAsDirty();
    }
    public addStep() {
        this.group.push(StepForm.getForm(this.formBuilder, null));
        this.group.markAsDirty();
    }
    public removeStep(index: number) {
        if (!this.dialogService.confirm('Are you sure you want to delete the step ?')) {
            return;
        }
        this.group.removeAt(index);
        this.group.markAsDirty();
    }
    public ngOnDestroy() {
        this._runtimeStoreSubscription.unsubscribe();
    }
    public stepChanged(formGroup: FormGroup, selectedStep: string) {
        let stepProps = this.steps.find((st) => st.technicalName === selectedStep);
        const setting = formGroup.get('setting');
        if (!!setting) {
            formGroup.removeControl('setting');
        }
        if (!!stepProps && stepProps.properties.length > 0) {
            formGroup
                .addControl('setting', this.formBuilder.array(stepProps
                    .properties
                    .map((prop) => SettingForm.getForm(this.formBuilder, {
                        key: prop.friendlyName,
                        value: ''
                    }))));
        }
        this._changeDetectorRef.detectChanges();
    }
}
