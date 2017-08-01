import { Injectable, Injector } from '@angular/core';
import { Subscription } from 'rxjs/Subscription';
import { FormBuilder, FormGroup, FormControl, AbstractControl } from '@angular/forms';
import 'rxjs/add/operator/distinctUntilChanged';

import { jsonAccessor } from '../common/jsonAccessor';
import { ItemType } from './../api/ItemType';

@Injectable()
export class FormBuilderExtended {
    constructor(private formBuilder: FormBuilder, private injector: Injector) { }
    public get(): FormWrapper {
        return new FormWrapper(this.formBuilder, '', this.injector);
    }
}

// tslint:disable-next-line:max-classes-per-file
export class FormWrapper {
    public form: FormGroup;
    private _onValueChangeSubscriptions = new Map<string | null, SubscriptionHandler>();
    private _onStatusChangeSubscriptions = new Map<string | null, SubscriptionHandler>();
    private _enabledConditions = new Map<string, string>();
    private _subs = new Map<string, FormWrapper>();
    private _buildTriggers = new Map<string, any | null>();
    constructor(public formBuilder: FormBuilder, public name: string = '', public injector: Injector) { }
    public jsonAccessor(data: any, path: string, defaultValue: any = null) {
        return jsonAccessor(data, path, defaultValue);
    }
    public createFieldValue(current: any, field: string, path: string, defaultValue: any = null, itemTypes: ItemType[]): any {
        const fullPath = `${path.toLocaleLowerCase()}.${field.toLowerCase()}`;
        let result = current == null || current[field] == null ? itemTypes[fullPath] === undefined ? defaultValue : itemTypes[fullPath].defaultvalue : current[field];
        return result;
    }
    public subForm(field: string): FormWrapper {
        const sub = this._subs.get(field);
        if (!!sub) {
            return sub;
        }

        const wrapper = new FormWrapper(this.formBuilder, field, this.injector);
        this._subs.set(field, wrapper);
        return wrapper;
    }
    public onChange<T>(field: string = this.name, handler: (current: T, wrapper: FormWrapper) => void): FormWrapper {
        this.unsubscribeHandler(field, this._onValueChangeSubscriptions)
            ._onValueChangeSubscriptions.set(field, new SubscriptionHandler({
                handler
            }));
        return this;
    }
    // tslint:disable-next-line:max-line-length
    public onStatusChange<T extends 'VALID' | 'INVALID' | 'DISABLED'>(field: string = this.name, handler: (current: 'VALID' | 'INVALID' | 'DISABLED', formWrapper: FormWrapper) => void): FormWrapper {
        this.unsubscribeHandler(field, this._onStatusChangeSubscriptions)
            ._onStatusChangeSubscriptions.set(field, new SubscriptionHandler({
                handler
            }));
        return this;
    }
    public setControl(field: string, control: FormGroup | FormControl): FormWrapper {
        this.form.removeControl(field);
        this.form.addControl(field, control);
        this.reApplyHandlers(field);
        return this;
    }
    public cleanup(): void {
        for (const subscription of Array.from(this._onValueChangeSubscriptions.entries())) {
            if (!!subscription[1].subscription) {
                subscription[1]!.subscription!.unsubscribe();
            }
        }

        for (const subscription of Array.from(this._onStatusChangeSubscriptions.entries())) {
            if (!!subscription[1].subscription) {
                subscription[1]!.subscription!.unsubscribe();
            }
        }

        // Cleanup sub forms
        for (const sub of Array.from(this._subs.entries())) {
            sub[1].cleanup();
        }

        this._onValueChangeSubscriptions = new Map<string, SubscriptionHandler>();
        this._onStatusChangeSubscriptions = new Map<string, SubscriptionHandler>();
        this._enabledConditions = new Map<string, string>();
        this._subs = new Map<string, FormWrapper>();
    }
    public group(controlsConfig: {
        [key: string]: any;
    }, extra?: {
        [key: string]: any;
    } | null): FormWrapper {
        const form = this.formBuilder.group(controlsConfig, extra);
        this.form = form;
        return this;
    }
    public build(isDisabled: boolean = false): FormGroup {
        if (isDisabled) {
            this.disable();
        } else {
            this.enable();
        }
        this.reApplyHandlers();

        return this.form;
    }
    public disable(except: string[] | null = null) {
        Object
            .keys(this.form.controls)
            .filter((key) => !!!except ? true : except.findIndex((search) => search === key) === -1)
            .forEach((key) => {
                this!.form!.get(key)!.disable();
            });
    }
    public enable(except: string[] | null = null) {
        Object
            .keys(this.form.controls)
            .filter((key) => !!!except ? true : except.findIndex((search) => search === key) === -1)
            .forEach((key) => {
                this.form!.get(key)!.enable();
            });
    }
    public triggerHandler(field: string, value: any | null = null): FormWrapper {
        this._buildTriggers.set(field, value);
        return this;
    }
    public reApplyHandlers(field: string | null = null) {
        let keys = Object.keys(this.form.controls);
        if (!!this.name) {
            keys = keys.concat(this.name);
        }

        if (!!field) {
            keys = keys.filter((key) => key === field);
        }

        for (const trigger of Array.from(this._buildTriggers.entries()).reverse()) {
            const valueHandler = this._onValueChangeSubscriptions.get(trigger[0]);
            const statusHandler = this._onStatusChangeSubscriptions.get(trigger[0]);
            if (!!valueHandler) {
                valueHandler.handler(this.getForm(trigger[0])!.value, this);
            }
            if (!!statusHandler) {
                statusHandler.handler(this.getForm(trigger[0])!.value, this);
            }
        }

        keys.forEach((key) => {
            const sub = this.getSub(key);
            if (!!sub) {
                // This is a 'sub' form, call reApplyHandlers on the sub
                sub.reApplyHandlers();
                return;
            }

            const valueChangeHandler = this._onValueChangeSubscriptions.get(key);
            if (!!valueChangeHandler) {
                if (!!valueChangeHandler.subscription) {
                    valueChangeHandler.subscription.unsubscribe();
                    valueChangeHandler.subscription = null;
                }
                valueChangeHandler.subscription = this
                    .getForm(key)
                    .valueChanges
                    .distinctUntilChanged()
                    .subscribe((result) => valueChangeHandler.handler(result, this));
            }

            const statusChangeHandler = this._onStatusChangeSubscriptions.get(key);
            if (!!statusChangeHandler) {
                if (!!statusChangeHandler.subscription) {
                    statusChangeHandler.subscription.unsubscribe();
                    statusChangeHandler.subscription = null;
                }
                statusChangeHandler.subscription = this
                    .getForm(key)
                    .statusChanges
                    .distinctUntilChanged()
                    .subscribe((result) => statusChangeHandler.handler(result, this));
            }
        });
    }
    private getSub(field: string): FormWrapper {
        return this._subs!.get(field)!;
    }
    private unsubscribeHandler(field: string, handlers: Map<string | null, SubscriptionHandler>): FormWrapper {
        const existing = handlers.get(field);
        if (!!existing && existing.subscription) {
            existing.subscription.unsubscribe();
            existing.subscription = null;
        }
        return this;
    }
    private getForm(name: string = this.name): AbstractControl {
        if (name === this.name) {
            return this.form;
        }
        return this.form.get(name)!;
    }
}

// tslint:disable-next-line:max-classes-per-file
class SubscriptionHandler {
    public subscription: Subscription | null;
    public handler: (current: any, wrapper: FormWrapper) => void;
    constructor(init: Partial<SubscriptionHandler>) {
        Object.assign(this, init);
    }
}
