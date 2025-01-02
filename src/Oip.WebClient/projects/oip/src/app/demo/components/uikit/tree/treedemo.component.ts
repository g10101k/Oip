import { Component, OnInit } from '@angular/core';
import { NodeService } from 'projects/oip/src/app/demo/service/node.service';
import { TreeNode, PrimeTemplate } from 'primeng/api';
import { Tree } from 'primeng/tree';
import { TreeTableModule } from 'primeng/treetable';
import { NgFor, NgIf } from '@angular/common';

@Component({
    templateUrl: './treedemo.component.html',
    imports: [Tree, TreeTableModule, PrimeTemplate, NgFor, NgIf]
})
export class TreeDemoComponent implements OnInit {

  files1: TreeNode[] = [];

  files2: TreeNode[] = [];

  files3: TreeNode[] = [];

  selectedFiles1: TreeNode[] = [];

  selectedFiles2: TreeNode[] = [];

  selectedFiles3: TreeNode = {};

  cols: any[] = [];

  constructor(private nodeService: NodeService) {
  }

  ngOnInit() {
    this.nodeService.getFiles().then(files => this.files1 = files);
    this.nodeService.getFilesystem().then(files => this.files2 = files);
    this.nodeService.getFiles().then(files => {
      this.files3 = [{
        label: 'Root',
        children: files
      }];
    });

    this.cols = [
      { field: 'name', header: 'Name' },
      { field: 'size', header: 'Size' },
      { field: 'type', header: 'Type' }
    ];
  }
}
