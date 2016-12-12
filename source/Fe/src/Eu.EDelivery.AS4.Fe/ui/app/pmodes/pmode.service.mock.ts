import { SendingPmode } from './../api/SendingPmode';
import { Observable } from 'rxjs';
import { Receiver } from './../api/Receiver';
import { IPmodeService } from './pmode.service';

import { ReceivingPmode } from './../api/ReceivingPmode';

export class PmodeServiceMock implements IPmodeService {
    getAllReceiving() { }
    getReceiving() { }
    getAllSending() { }
    getSending() { }
    deleteReceiving(name: string) { }
    deleteSending(name: string) { }
    createReceiving(pmode: ReceivingPmode): Observable<boolean> { return null; }
    updateReceiving(pmode: ReceivingPmode, originalName: string): Observable<boolean> { return null; }
    createSending(pmode: SendingPmode): Observable<boolean> { return null; }
    updateSending(pmode: SendingPmode, originalName: string): Observable<boolean> { return null; }
}
