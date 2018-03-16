import { AfterViewInit, Directive, ElementRef, Input, OnDestroy, Renderer } from '@angular/core';
import { FormGroup } from '@angular/forms';
import { BehaviorSubject } from 'rxjs/BehaviorSubject';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs/Subscription';

@Directive({
    selector: '[tabitem]'
})
export class TabItemDirective implements AfterViewInit, OnDestroy {
    @Input() public title: string;
    @Input() public tabId: number;
    @Input() public set isInvalid(isInvalid: boolean) {
        this._isValidTab.next(!isInvalid);
    }
    @Input() public set isValid(isValid: boolean | FormGroup) {
        this.cleanup();
        if (isValid instanceof FormGroup) {
            this.cleanup();
            this._isValidTab.next(isValid.status === 'VALID' || isValid.status === 'DISABLED');
            return;
        }
        this._isValidTab.next(isValid);
    }
    public isValidTab: Observable<boolean>;
    private _isValidTab = new BehaviorSubject<boolean>(true);
    private _subscriptions: Subscription;
    constructor(private _elementRef: ElementRef, private _renderer: Renderer) {
        _renderer.setElementClass(this._elementRef.nativeElement, 'tab-pane', true);
        this.isValidTab = this._isValidTab.asObservable();
    }
    public ngAfterViewInit() {
        this._renderer.setElementAttribute(this._elementRef.nativeElement, 'id', `tab_${this.tabId}`);
        if (this.tabId === 0) {
            this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
        }
    }
    public ngOnDestroy() {
        this.cleanup();
    }
    public setActive() {
        this._renderer.setElementClass(this._elementRef.nativeElement, 'active', true);
    }
    public setInactive() {
        this._renderer.setElementClass(this._elementRef.nativeElement, 'active', false);
    }
    private cleanup() {
        if (!!this._subscriptions) {
            this._subscriptions.unsubscribe();
        }
    }
}
