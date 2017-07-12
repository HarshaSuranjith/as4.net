import { Component, OnInit, Input, Output, EventEmitter, ChangeDetectionStrategy } from '@angular/core';
import { FormGroup } from '@angular/forms';

@Component({
    selector: '[crud-buttons]',
    template: `
        <button type="button" as4-auth class="btn btn-flat rename-button" (click)="rename.emit()" [disabled]="!!!current || null"><i class="fa fa-edit"></i></button>
        <button type="button" as4-auth class="btn btn-flat add-button" (click)="add.emit()" [disabled]="isNewMode || null"><i class="fa fa-plus"></i></button>
        <button type="button" as4-auth class="btn btn-flat save-button" (click)="save.emit()" [class.btn-primary]="form.dirty || isNewMode" [disabled]="(!form.dirty && !isNewMode) || null"><i class="fa fa-save"></i></button>
        <button type="button" as4-auth class="btn btn-flat delete-button" (click)="delete.emit()" [disabled]="!!!current || null"><i class="fa fa-trash-o"></i></button>
        <button type="button" as4-auth class="btn btn-flat reset-button" (click)="reset.emit()" [class.btn-primary]="form.dirty || isNewMode" [disabled]="(!form.dirty && !isNewMode) || null"><i class="fa fa-undo"></i></button>
        <ng-content></ng-content>
    `
})
export class CrudButtonsComponent {
    @Output() public rename = new EventEmitter();
    @Output() public add = new EventEmitter();
    @Output() public save = new EventEmitter();
    @Output() public delete = new EventEmitter();
    @Output() public reset = new EventEmitter();
    @Input() public current: string;
    @Input() public form: FormGroup;
    @Input() public isNewMode: boolean = false;
}
