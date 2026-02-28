import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import { FilterMetadata } from 'primeng/api';
import { Table } from 'primeng/table';

/**
 * Prime-ng table filter service
 */
@Injectable()
export class TableFilterService {
  public filterState: { [field: string]: boolean } = {};
  private filtersState = new BehaviorSubject<boolean>(false);
  public filtersState$ = this.filtersState.asObservable();

  /**
   * Updates filter state
   * @param filters
   */
  public updateFiltersState(filters: any) {
    const hasFilters = Object.values(filters).some((filterArray: any) => {
      if (Array.isArray(filterArray)) {
        return filterArray.some((filter) => !!filter.value);
      }
      return false;
    });
    this.filtersState.next(hasFilters);
  }

  /**
   * Checks if a filter is active
   * @param {FilterMetadata | FilterMetadata[]} filter
   * @return {boolean}
   */
  public isFilterActive(filter: FilterMetadata | FilterMetadata[]): boolean {
    if (!filter) return false;

    if (Array.isArray(filter)) {
      return filter.some((f) => !!f.value && f.matchMode !== 'unspecified');
    } else {
      return !!filter.value && filter.matchMode !== 'unspecified';
    }
  }

  /**
   * Checks the state of table filters
   * @param {Table} table
   * @param {FilterMetadata | FilterMetadata[]} filters
   */
  public updateFilterFieldStates(table: Table, filters: FilterMetadata | FilterMetadata[]) {
    if (table && filters) {
      Object.keys(filters).forEach((field) => {
        const filter = filters[field];
        this.filterState[field] = this.isFilterActive(filter);
      });
    }
    this.updateFiltersState(filters);
  }
}
