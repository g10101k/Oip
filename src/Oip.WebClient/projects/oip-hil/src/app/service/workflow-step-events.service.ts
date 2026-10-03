import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { CompleteUserStepResponse } from '../../api/data-contracts';

/**
 * Lets a workflow step page (`WorkflowTaskComponent`, `WorkflowFormComponent`) tell the task list that a step was
 * completed without the list knowing which step components exist.
 */
@Injectable({ providedIn: 'root' })
export class WorkflowStepEventsService {
  private readonly completedSubject = new Subject<CompleteUserStepResponse>();

  /** Emits after a step resumed its workflow. */
  readonly completed$ = this.completedSubject.asObservable();

  notifyCompleted(response: CompleteUserStepResponse): void {
    this.completedSubject.next(response);
  }
}
