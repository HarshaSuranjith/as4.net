import { Observable } from 'rxjs/Observable';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Component, Input, Output } from '@angular/core';
import { FormBuilder, FormGroup } from '@angular/forms';

import { RuntimeStore } from './runtime.store';
import { CanComponentDeactivate } from './../common/candeactivate.guard';
import { DialogService } from './../common/dialog.service';
import { Base } from './../api/Base';
import { BaseForm } from './../api/BaseForm';
import { SettingsService } from './settings.service';
import { ItemType } from './../api/ItemType';
import '../common/rxjs/toBehaviorSubject';

@Component({
    selector: 'as4-base-settings',
    template: `
        <form [formGroup]="form" class="form-horizontal">
            <as4-input [label]="'Message Id'">
                <input type="text" class="form-control" name="idFormat" formControlName="idFormat"/>
            </as4-input>
            <as4-input [tooltip]="'Defines at which interval the messages should be cleaned up'" [label]="'Retention Period (in days)'">
                <input type="number" class="form-control" name="retentionPeriod" formControlName="retentionPeriod" />
            </as4-input>
            <as4-input [label]="'Certificate store'" formGroupName="certificateStore">
                <input type="text" class="form-control" name="certificateStoreName" formControlName="storeName"/>
            </as4-input>            
            <div formGroupName="certificateStore">
                <as4-input [label]="'Certificate retriever'" formGroupName="repository">
                    <select class="form-control" formControlName="type">
                        <option *ngFor="let repository of repositories" [value]="repository.technicalName">{{repository.name}}</option>
                    </select>
                </as4-input>
            </div>
        </form>
    `
})
export class BaseSettingsComponent implements CanComponentDeactivate {
    public form: FormGroup;
    @Output() public isDirty: Observable<boolean>;
    @Input() public get settings(): Base {
        return this._settings;
    }
    public repositories: ItemType[];
    public set settings(baseSetting: Base) {
        this.form = BaseForm.getForm(this.formBuilder, baseSetting);
        this.isDirty = this
            .form
            .valueChanges
            .map(() => this.form.dirty)
            .toBehaviorSubject(this.form.dirty);
        this._settings = baseSetting;
    }
    private _settings: Base;
    constructor(private settingsService: SettingsService, private formBuilder: FormBuilder, private runtimeStore: RuntimeStore, private dialogService: DialogService) {
        runtimeStore
            .changes
            .filter((result) => !!(result && result.certificateRepositories))
            .subscribe((result) => this.repositories = result.certificateRepositories);
    }
    public save() {
        if (!this.form.valid) {
            this.dialogService.incorrectForm();
            return;
        }
        this.settingsService
            .saveBaseSettings(this.form.value)
            .subscribe((result) => {
                this.form.markAsPristine();
                this.form.updateValueAndValidity();
                this.dialogService.message(`Settings will only be applied after restarting the runtime!`, 'Attention');
            });
    }
    public canDeactivate(): boolean {
        return !this.form.dirty;
    }
}
