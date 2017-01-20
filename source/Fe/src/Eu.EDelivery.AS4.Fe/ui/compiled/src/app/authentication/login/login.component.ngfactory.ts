/**
 * @fileoverview This file is generated by the Angular 2 template compiler.
 * Do not edit.
 * @suppress {suspiciousCode,uselessCode,missingProperties}
 */
 /* tslint:disable */

import * as import0 from '../../../../../src/app/authentication/login/login.component';
import * as import1 from '@angular/core/src/linker/view';
import * as import2 from '@angular/core/src/render/api';
import * as import3 from '@angular/core/src/linker/view_utils';
import * as import4 from '@angular/core/src/metadata/view';
import * as import5 from '@angular/core/src/linker/view_type';
import * as import6 from '@angular/core/src/change_detection/constants';
import * as import7 from '@angular/core/src/linker/component_factory';
import * as import8 from '@angular/http/src/http';
import * as import9 from '@angular/router/src/router_state';
import * as import10 from '../../../../../src/app/authentication/authentication.service';
import * as import11 from '../../../../node_modules/@angular/forms/src/directives/ng_form.ngfactory';
import * as import12 from '../../../../node_modules/@angular/forms/src/directives/ng_control_status.ngfactory';
import * as import13 from '../../../../node_modules/@angular/forms/src/directives/default_value_accessor.ngfactory';
import * as import14 from '../../../../node_modules/@angular/forms/src/directives/ng_model.ngfactory';
import * as import15 from '@angular/core/src/linker/element_ref';
import * as import16 from '@angular/forms/src/directives/default_value_accessor';
import * as import17 from '@angular/forms/src/directives/control_value_accessor';
import * as import18 from '@angular/forms/src/directives/ng_model';
import * as import19 from '@angular/forms/src/directives/ng_control';
import * as import20 from '@angular/forms/src/directives/ng_control_status';
import * as import21 from '@angular/forms/src/directives/ng_form';
import * as import22 from '@angular/forms/src/directives/control_container';
export class Wrapper_LoginComponent {
  /*private*/ _eventHandler:Function;
  context:import0.LoginComponent;
  /*private*/ _changed:boolean;
  constructor(p0:any,p1:any,p2:any,p3:any) {
    this._changed = false;
    this.context = new import0.LoginComponent(p0,p1,p2,p3);
  }
  ngOnDetach(view:import1.AppView<any>,componentView:import1.AppView<any>,el:any):void {
  }
  ngOnDestroy():void {
  }
  ngDoCheck(view:import1.AppView<any>,el:any,throwOnChange:boolean):boolean {
    var changed:any = this._changed;
    this._changed = false;
    if (!throwOnChange) { if ((view.numberOfChecks === 0)) { this.context.ngOnInit(); } }
    return changed;
  }
  checkHost(view:import1.AppView<any>,componentView:import1.AppView<any>,el:any,throwOnChange:boolean):void {
  }
  handleEvent(eventName:string,$event:any):boolean {
    var result:boolean = true;
    return result;
  }
  subscribe(view:import1.AppView<any>,_eventHandler:any):void {
    this._eventHandler = _eventHandler;
  }
}
var renderType_LoginComponent_Host:import2.RenderComponentType = import3.createRenderComponentType('',0,import4.ViewEncapsulation.None,([] as any[]),{});
class View_LoginComponent_Host0 extends import1.AppView<any> {
  _el_0:any;
  compView_0:import1.AppView<import0.LoginComponent>;
  _LoginComponent_0_3:Wrapper_LoginComponent;
  constructor(viewUtils:import3.ViewUtils,parentView:import1.AppView<any>,parentIndex:number,parentElement:any) {
    super(View_LoginComponent_Host0,renderType_LoginComponent_Host,import5.ViewType.HOST,viewUtils,parentView,parentIndex,parentElement,import6.ChangeDetectorStatus.CheckAlways);
  }
  createInternal(rootSelector:string):import7.ComponentRef<any> {
    this._el_0 = import3.selectOrCreateRenderHostElement(this.renderer,'as4-login',import3.EMPTY_INLINE_ARRAY,rootSelector,(null as any));
    this.compView_0 = new View_LoginComponent0(this.viewUtils,this,0,this._el_0);
    this._LoginComponent_0_3 = new Wrapper_LoginComponent(this.injectorGet(import8.Http,this.parentIndex),this.injectorGet(import9.ActivatedRoute,this.parentIndex),this.injectorGet(import10.AuthenticationService,this.parentIndex),this.injectorGet(import10.AuthenticationStore,this.parentIndex));
    this.compView_0.create(this._LoginComponent_0_3.context);
    this.init(this._el_0,((<any>this.renderer).directRenderer? (null as any): [this._el_0]),(null as any));
    return new import7.ComponentRef_<any>(0,this,this._el_0,this._LoginComponent_0_3.context);
  }
  injectorGetInternal(token:any,requestNodeIndex:number,notFoundResult:any):any {
    if (((token === import0.LoginComponent) && (0 === requestNodeIndex))) { return this._LoginComponent_0_3.context; }
    return notFoundResult;
  }
  detectChangesInternal(throwOnChange:boolean):void {
    if (this._LoginComponent_0_3.ngDoCheck(this,this._el_0,throwOnChange)) { this.compView_0.markAsCheckOnce(); }
    this.compView_0.internalDetectChanges(throwOnChange);
  }
  destroyInternal():void {
    this.compView_0.destroy();
  }
  visitRootNodesInternal(cb:any,ctx:any):void {
    cb(this._el_0,ctx);
  }
}
export const LoginComponentNgFactory:import7.ComponentFactory<import0.LoginComponent> = new import7.ComponentFactory<import0.LoginComponent>('as4-login',View_LoginComponent_Host0,import0.LoginComponent);
const styles_LoginComponent:any[] = ([] as any[]);
var renderType_LoginComponent:import2.RenderComponentType = import3.createRenderComponentType('',0,import4.ViewEncapsulation.None,styles_LoginComponent,{});
export class View_LoginComponent0 extends import1.AppView<import0.LoginComponent> {
  _el_0:any;
  _text_1:any;
  _el_2:any;
  _text_3:any;
  _text_4:any;
  _el_5:any;
  _text_6:any;
  _el_7:any;
  _text_8:any;
  _text_9:any;
  _el_10:any;
  _NgForm_10_3:import11.Wrapper_NgForm;
  _ControlContainer_10_4:any;
  _NgControlStatusGroup_10_5:import12.Wrapper_NgControlStatusGroup;
  _text_11:any;
  _el_12:any;
  _text_13:any;
  _el_14:any;
  _DefaultValueAccessor_14_3:import13.Wrapper_DefaultValueAccessor;
  _NG_VALUE_ACCESSOR_14_4:any[];
  _NgModel_14_5:import14.Wrapper_NgModel;
  _NgControl_14_6:any;
  _NgControlStatus_14_7:import12.Wrapper_NgControlStatus;
  _text_15:any;
  _el_16:any;
  _text_17:any;
  _text_18:any;
  _el_19:any;
  _text_20:any;
  _el_21:any;
  _DefaultValueAccessor_21_3:import13.Wrapper_DefaultValueAccessor;
  _NG_VALUE_ACCESSOR_21_4:any[];
  _NgModel_21_5:import14.Wrapper_NgModel;
  _NgControl_21_6:any;
  _NgControlStatus_21_7:import12.Wrapper_NgControlStatus;
  _text_22:any;
  _el_23:any;
  _text_24:any;
  _text_25:any;
  _el_26:any;
  _text_27:any;
  _el_28:any;
  _text_29:any;
  _el_30:any;
  _text_31:any;
  _text_32:any;
  _text_33:any;
  _text_34:any;
  _text_35:any;
  _el_36:any;
  _text_37:any;
  _el_38:any;
  _text_39:any;
  _text_40:any;
  _el_41:any;
  _el_42:any;
  _text_43:any;
  _text_44:any;
  _text_45:any;
  _text_46:any;
  constructor(viewUtils:import3.ViewUtils,parentView:import1.AppView<any>,parentIndex:number,parentElement:any) {
    super(View_LoginComponent0,renderType_LoginComponent,import5.ViewType.COMPONENT,viewUtils,parentView,parentIndex,parentElement,import6.ChangeDetectorStatus.CheckOnce);
  }
  createInternal(rootSelector:string):import7.ComponentRef<any> {
    const parentRenderNode:any = this.renderer.createViewRoot(this.parentElement);
    this._el_0 = import3.createRenderElement(this.renderer,parentRenderNode,'div',new import3.InlineArray2(2,'class','login-box'),(null as any));
    this._text_1 = this.renderer.createText(this._el_0,'\n    ',(null as any));
    this._el_2 = import3.createRenderElement(this.renderer,this._el_0,'div',new import3.InlineArray2(2,'class','login-logo'),(null as any));
    this._text_3 = this.renderer.createText(this._el_2,'\n        AS4.NET\n    ',(null as any));
    this._text_4 = this.renderer.createText(this._el_0,'\n    ',(null as any));
    this._el_5 = import3.createRenderElement(this.renderer,this._el_0,'div',new import3.InlineArray2(2,'class','login-box-body'),(null as any));
    this._text_6 = this.renderer.createText(this._el_5,'\n        ',(null as any));
    this._el_7 = import3.createRenderElement(this.renderer,this._el_5,'p',new import3.InlineArray2(2,'class','login-box-msg'),(null as any));
    this._text_8 = this.renderer.createText(this._el_7,'Sign in to start your session',(null as any));
    this._text_9 = this.renderer.createText(this._el_5,'\n\n        ',(null as any));
    this._el_10 = import3.createRenderElement(this.renderer,this._el_5,'form',import3.EMPTY_INLINE_ARRAY,(null as any));
    this._NgForm_10_3 = new import11.Wrapper_NgForm((null as any),(null as any));
    this._ControlContainer_10_4 = this._NgForm_10_3.context;
    this._NgControlStatusGroup_10_5 = new import12.Wrapper_NgControlStatusGroup(this._ControlContainer_10_4);
    this._text_11 = this.renderer.createText(this._el_10,'\n            ',(null as any));
    this._el_12 = import3.createRenderElement(this.renderer,this._el_10,'div',new import3.InlineArray2(2,'class','form-group has-feedback'),(null as any));
    this._text_13 = this.renderer.createText(this._el_12,'\n                ',(null as any));
    this._el_14 = import3.createRenderElement(this.renderer,this._el_12,'input',new import3.InlineArray8(8,'class','form-control','name','username','placeholder','Username','type','username'),(null as any));
    this._DefaultValueAccessor_14_3 = new import13.Wrapper_DefaultValueAccessor(this.renderer,new import15.ElementRef(this._el_14));
    this._NG_VALUE_ACCESSOR_14_4 = [this._DefaultValueAccessor_14_3.context];
    this._NgModel_14_5 = new import14.Wrapper_NgModel(this._ControlContainer_10_4,(null as any),(null as any),this._NG_VALUE_ACCESSOR_14_4);
    this._NgControl_14_6 = this._NgModel_14_5.context;
    this._NgControlStatus_14_7 = new import12.Wrapper_NgControlStatus(this._NgControl_14_6);
    this._text_15 = this.renderer.createText(this._el_12,'\n                ',(null as any));
    this._el_16 = import3.createRenderElement(this.renderer,this._el_12,'span',new import3.InlineArray2(2,'class','glyphicon glyphicon-envelope form-control-feedback'),(null as any));
    this._text_17 = this.renderer.createText(this._el_12,'\n            ',(null as any));
    this._text_18 = this.renderer.createText(this._el_10,'\n            ',(null as any));
    this._el_19 = import3.createRenderElement(this.renderer,this._el_10,'div',new import3.InlineArray2(2,'class','form-group has-feedback'),(null as any));
    this._text_20 = this.renderer.createText(this._el_19,'\n                ',(null as any));
    this._el_21 = import3.createRenderElement(this.renderer,this._el_19,'input',new import3.InlineArray8(8,'class','form-control','name','password','placeholder','Password','type','password'),(null as any));
    this._DefaultValueAccessor_21_3 = new import13.Wrapper_DefaultValueAccessor(this.renderer,new import15.ElementRef(this._el_21));
    this._NG_VALUE_ACCESSOR_21_4 = [this._DefaultValueAccessor_21_3.context];
    this._NgModel_21_5 = new import14.Wrapper_NgModel(this._ControlContainer_10_4,(null as any),(null as any),this._NG_VALUE_ACCESSOR_21_4);
    this._NgControl_21_6 = this._NgModel_21_5.context;
    this._NgControlStatus_21_7 = new import12.Wrapper_NgControlStatus(this._NgControl_21_6);
    this._text_22 = this.renderer.createText(this._el_19,'\n                ',(null as any));
    this._el_23 = import3.createRenderElement(this.renderer,this._el_19,'span',new import3.InlineArray2(2,'class','glyphicon glyphicon-lock form-control-feedback'),(null as any));
    this._text_24 = this.renderer.createText(this._el_19,'\n            ',(null as any));
    this._text_25 = this.renderer.createText(this._el_10,'\n            ',(null as any));
    this._el_26 = import3.createRenderElement(this.renderer,this._el_10,'div',new import3.InlineArray2(2,'class','row'),(null as any));
    this._text_27 = this.renderer.createText(this._el_26,'\n                ',(null as any));
    this._el_28 = import3.createRenderElement(this.renderer,this._el_26,'div',new import3.InlineArray2(2,'class','col-xs-4 pull-right'),(null as any));
    this._text_29 = this.renderer.createText(this._el_28,'\n                    ',(null as any));
    this._el_30 = import3.createRenderElement(this.renderer,this._el_28,'button',new import3.InlineArray4(4,'class','btn btn-primary btn-block btn-flat','type','submit'),(null as any));
    this._text_31 = this.renderer.createText(this._el_30,'Sign In',(null as any));
    this._text_32 = this.renderer.createText(this._el_28,'\n                ',(null as any));
    this._text_33 = this.renderer.createText(this._el_26,'\n            ',(null as any));
    this._text_34 = this.renderer.createText(this._el_10,'\n        ',(null as any));
    this._text_35 = this.renderer.createText(this._el_5,'\n\n        ',(null as any));
    this._el_36 = import3.createRenderElement(this.renderer,this._el_5,'div',new import3.InlineArray2(2,'class','social-auth-links text-center'),(null as any));
    this._text_37 = this.renderer.createText(this._el_36,'\n            ',(null as any));
    this._el_38 = import3.createRenderElement(this.renderer,this._el_36,'p',import3.EMPTY_INLINE_ARRAY,(null as any));
    this._text_39 = this.renderer.createText(this._el_38,'- OR -',(null as any));
    this._text_40 = this.renderer.createText(this._el_36,'\n            ',(null as any));
    this._el_41 = import3.createRenderElement(this.renderer,this._el_36,'a',new import3.InlineArray2(2,'class','btn btn-block btn-social btn-facebook btn-flat'),(null as any));
    this._el_42 = import3.createRenderElement(this.renderer,this._el_41,'i',new import3.InlineArray2(2,'class','fa fa-facebook'),(null as any));
    this._text_43 = this.renderer.createText(this._el_41,' Sign in using Facebook',(null as any));
    this._text_44 = this.renderer.createText(this._el_36,'\n        ',(null as any));
    this._text_45 = this.renderer.createText(this._el_5,'\n    ',(null as any));
    this._text_46 = this.renderer.createText(this._el_0,'\n',(null as any));
    var disposable_0:Function = import3.subscribeToRenderElement(this,this._el_10,new import3.InlineArray4(4,'submit',(null as any),'reset',(null as any)),this.eventHandler(this.handleEvent_10));
    var disposable_1:Function = import3.subscribeToRenderElement(this,this._el_14,new import3.InlineArray8(6,'ngModelChange',(null as any),'input',(null as any),'blur',(null as any)),this.eventHandler(this.handleEvent_14));
    this._NgModel_14_5.subscribe(this,this.eventHandler(this.handleEvent_14),true);
    var disposable_2:Function = import3.subscribeToRenderElement(this,this._el_21,new import3.InlineArray8(6,'ngModelChange',(null as any),'input',(null as any),'blur',(null as any)),this.eventHandler(this.handleEvent_21));
    this._NgModel_21_5.subscribe(this,this.eventHandler(this.handleEvent_21),true);
    var disposable_3:Function = import3.subscribeToRenderElement(this,this._el_30,new import3.InlineArray2(2,'click',(null as any)),this.eventHandler(this.handleEvent_30));
    var disposable_4:Function = import3.subscribeToRenderElement(this,this._el_41,new import3.InlineArray2(2,'click',(null as any)),this.eventHandler(this.handleEvent_41));
    this.init((null as any),((<any>this.renderer).directRenderer? (null as any): [
      this._el_0,
      this._text_1,
      this._el_2,
      this._text_3,
      this._text_4,
      this._el_5,
      this._text_6,
      this._el_7,
      this._text_8,
      this._text_9,
      this._el_10,
      this._text_11,
      this._el_12,
      this._text_13,
      this._el_14,
      this._text_15,
      this._el_16,
      this._text_17,
      this._text_18,
      this._el_19,
      this._text_20,
      this._el_21,
      this._text_22,
      this._el_23,
      this._text_24,
      this._text_25,
      this._el_26,
      this._text_27,
      this._el_28,
      this._text_29,
      this._el_30,
      this._text_31,
      this._text_32,
      this._text_33,
      this._text_34,
      this._text_35,
      this._el_36,
      this._text_37,
      this._el_38,
      this._text_39,
      this._text_40,
      this._el_41,
      this._el_42,
      this._text_43,
      this._text_44,
      this._text_45,
      this._text_46
    ]
    ),[
      disposable_0,
      disposable_1,
      disposable_2,
      disposable_3,
      disposable_4
    ]
    );
    return (null as any);
  }
  injectorGetInternal(token:any,requestNodeIndex:number,notFoundResult:any):any {
    if (((token === import16.DefaultValueAccessor) && (14 === requestNodeIndex))) { return this._DefaultValueAccessor_14_3.context; }
    if (((token === import17.NG_VALUE_ACCESSOR) && (14 === requestNodeIndex))) { return this._NG_VALUE_ACCESSOR_14_4; }
    if (((token === import18.NgModel) && (14 === requestNodeIndex))) { return this._NgModel_14_5.context; }
    if (((token === import19.NgControl) && (14 === requestNodeIndex))) { return this._NgControl_14_6; }
    if (((token === import20.NgControlStatus) && (14 === requestNodeIndex))) { return this._NgControlStatus_14_7.context; }
    if (((token === import16.DefaultValueAccessor) && (21 === requestNodeIndex))) { return this._DefaultValueAccessor_21_3.context; }
    if (((token === import17.NG_VALUE_ACCESSOR) && (21 === requestNodeIndex))) { return this._NG_VALUE_ACCESSOR_21_4; }
    if (((token === import18.NgModel) && (21 === requestNodeIndex))) { return this._NgModel_21_5.context; }
    if (((token === import19.NgControl) && (21 === requestNodeIndex))) { return this._NgControl_21_6; }
    if (((token === import20.NgControlStatus) && (21 === requestNodeIndex))) { return this._NgControlStatus_21_7.context; }
    if (((token === import21.NgForm) && ((10 <= requestNodeIndex) && (requestNodeIndex <= 34)))) { return this._NgForm_10_3.context; }
    if (((token === import22.ControlContainer) && ((10 <= requestNodeIndex) && (requestNodeIndex <= 34)))) { return this._ControlContainer_10_4; }
    if (((token === import20.NgControlStatusGroup) && ((10 <= requestNodeIndex) && (requestNodeIndex <= 34)))) { return this._NgControlStatusGroup_10_5.context; }
    return notFoundResult;
  }
  detectChangesInternal(throwOnChange:boolean):void {
    this._NgForm_10_3.ngDoCheck(this,this._el_10,throwOnChange);
    this._NgControlStatusGroup_10_5.ngDoCheck(this,this._el_10,throwOnChange);
    this._DefaultValueAccessor_14_3.ngDoCheck(this,this._el_14,throwOnChange);
    const currVal_14_1_0:any = 'username';
    this._NgModel_14_5.check_name(currVal_14_1_0,throwOnChange,false);
    const currVal_14_1_1:any = this.context.username;
    this._NgModel_14_5.check_model(currVal_14_1_1,throwOnChange,false);
    this._NgModel_14_5.ngDoCheck(this,this._el_14,throwOnChange);
    this._NgControlStatus_14_7.ngDoCheck(this,this._el_14,throwOnChange);
    this._DefaultValueAccessor_21_3.ngDoCheck(this,this._el_21,throwOnChange);
    const currVal_21_1_0:any = 'password';
    this._NgModel_21_5.check_name(currVal_21_1_0,throwOnChange,false);
    const currVal_21_1_1:any = this.context.password;
    this._NgModel_21_5.check_model(currVal_21_1_1,throwOnChange,false);
    this._NgModel_21_5.ngDoCheck(this,this._el_21,throwOnChange);
    this._NgControlStatus_21_7.ngDoCheck(this,this._el_21,throwOnChange);
    this._NgControlStatusGroup_10_5.checkHost(this,this,this._el_10,throwOnChange);
    this._NgControlStatus_14_7.checkHost(this,this,this._el_14,throwOnChange);
    this._NgControlStatus_21_7.checkHost(this,this,this._el_21,throwOnChange);
  }
  destroyInternal():void {
    this._NgModel_14_5.ngOnDestroy();
    this._NgModel_21_5.ngOnDestroy();
    this._NgForm_10_3.ngOnDestroy();
  }
  handleEvent_10(eventName:string,$event:any):boolean {
    this.markPathToRootAsCheckOnce();
    var result:boolean = true;
    result = (this._NgForm_10_3.handleEvent(eventName,$event) && result);
    return result;
  }
  handleEvent_14(eventName:string,$event:any):boolean {
    this.markPathToRootAsCheckOnce();
    var result:boolean = true;
    result = (this._DefaultValueAccessor_14_3.handleEvent(eventName,$event) && result);
    if ((eventName == 'ngModelChange')) {
      const pd_sub_0:any = ((<any>(this.context.username = $event)) !== false);
      result = (pd_sub_0 && result);
    }
    return result;
  }
  handleEvent_21(eventName:string,$event:any):boolean {
    this.markPathToRootAsCheckOnce();
    var result:boolean = true;
    result = (this._DefaultValueAccessor_21_3.handleEvent(eventName,$event) && result);
    if ((eventName == 'ngModelChange')) {
      const pd_sub_0:any = ((<any>(this.context.password = $event)) !== false);
      result = (pd_sub_0 && result);
    }
    return result;
  }
  handleEvent_30(eventName:string,$event:any):boolean {
    this.markPathToRootAsCheckOnce();
    var result:boolean = true;
    if ((eventName == 'click')) {
      const pd_sub_0:any = ((<any>this.context.login()) !== false);
      result = (pd_sub_0 && result);
    }
    return result;
  }
  handleEvent_41(eventName:string,$event:any):boolean {
    this.markPathToRootAsCheckOnce();
    var result:boolean = true;
    if ((eventName == 'click')) {
      const pd_sub_0:any = ((<any>this.context.login()) !== false);
      result = (pd_sub_0 && result);
    }
    return result;
  }
}