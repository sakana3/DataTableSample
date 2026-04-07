using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

    
namespace TinyDataTable.Editor
{
    public class SerializableTreeView<ITEM> : VisualElement where ITEM : class
    {
        public TreeView treeView { private set; get; }
        public UnityEditor.UIElements.ToolbarSearchField serchField { private set; get; } 
        public SerializableTree<ITEM> target;
        public event Action<List<SerializableTree<ITEM>.TreeNode>> hierarchyChanged;

        public Func<int,SerializableTree<ITEM>.Node,bool,bool, VisualElement> makeItem;
        public Action<Rect, Action<string,ITEM>> onCreateItem;
        public Action<ITEM> OnSelectDataTableAsset;
        
        public SerializableTreeView(SerializableTree<ITEM> target)
        {
            this.target = target;
            CreateGUI();
        }

        private void CreateGUI()
        {
            this.Clear();
            
            serchField = new UnityEditor.UIElements.ToolbarSearchField();
            serchField.style.width = new StyleLength( StyleKeyword.Auto );
            serchField.RegisterValueChangedCallback( OnSearchBarValueChangedCallback );
            this.Add(serchField);
            
            treeView = new TreeView()
            {
                selectionType = SelectionType.Single,
                reorderable = true,
            };
            treeView.style.flexGrow = 1;
            treeView.itemIndexChanged += (_,_) => OnHerarchyChanged();
            treeView.makeItem += () => new VisualElement();
            treeView.bindItem += (element, i) =>
            {
                element.Clear();
                if (makeItem == null)
                {
                    var label = new Label();
                    label.text = treeView.GetItemDataForId<SerializableTree<ITEM>.TreeNode>(i).node.Name;
                    element.Add(label);
                }
                else
                {
                    var node = treeView.GetItemDataForId<SerializableTree<ITEM>.TreeNode>(i).node;
                    bool isExpand = treeView.viewController.IsExpanded(i);
                    bool hasChildren = treeView.viewController.HasChildren(i);
                    var ve = makeItem.Invoke(i,node,isExpand,hasChildren);
                    element.Add(ve);
                }

                var itemContextMenu = new ContextualMenuManipulator((e) =>
                    {
                        e.menu.AppendAction("Create Folder", (p) => InsertNewTree(i, "New Folder"));
                        e.menu.AppendAction("Create Table", (p) => CreateItem(p.eventInfo.mousePosition,i));
                        e.menu.AppendAction("Delete", (p) => RemoveTree(i));
                    }
                ) { target = element };

            };
            var itemContextMenu = new ContextualMenuManipulator((e) =>
                {
                    e.menu.AppendAction("Create Folder", (p) =>
                    {
                        InsertNewTree(-1,"New Folder");
                    });
                    e.menu.AppendAction("Create Table", (p) =>
                    {
                        CreateItem(p.eventInfo.mousePosition,-1);
                    });
                }
            ) { target = treeView };
            treeView.itemExpandedChanged += args =>
            {
                treeView.RefreshItems();
            };
            treeView.selectedIndicesChanged += indexs =>
            {
                var index = indexs.FirstOrDefault();
                var node = treeView.GetItemDataForIndex<SerializableTree<ITEM>.TreeNode>(index);
                OnSelectDataTableAsset?.Invoke(node.node.Item);
            };
            
            treeView.viewDataKey = $"SerializableTreeView<{nameof(ITEM)}>";
            Add(treeView);

            
            
            BuildTree(target.ToTree());
        }

        public void BuildTree(SerializableTree<ITEM> item)
        {
            this.target = item;
            if (CheckTree(item))
            {
                RefreshTree(item);
            }
            else
            {
                BuildTree(item.ToTree());
            }
        }

        private void RefreshTree(SerializableTree<ITEM> tree)
        {
            Debug.Log("RefreshTree");
            var root = treeView.viewController.GetRootItemIds();

            for (int i = 0; i < tree.Nodes.Length; i++)
            {
                {
                    var item = treeView.viewController.GetItemForIndex(i) as SerializableTree<ITEM>.TreeNode;
                    item.node = tree.Nodes[i];
                    treeView.RefreshItem(i);
                }
            }
        }

        private void BuildTree(List<SerializableTree<ITEM>.TreeNode> tree)
        {
            Debug.Log("BuildTree");
            var root = MakeTree(target.ToTree());
            treeView.SetRootItems(root);
            treeView.Rebuild();
        }

        
        private void CreateItem( Vector2 positon, int rootID )
        {
            var mouseRect = new Rect(positon, Vector2.one);
            onCreateItem?.Invoke( mouseRect , (className,item) => InsertNewTree(rootID, className,item) );
        }
        
        private void InsertNewTree(int rootID, string name,ITEM nodeItem = null)
        {
            Debug.Log($"InsertNewTree {rootID}");
            int childIndex = -1;

            if (rootID >= 0)
            {
                var t = treeView.viewController.GetItemForId(rootID) as SerializableTree<ITEM>.TreeNode;
                if (t.node.IsFolder)
                {
                    childIndex = -1;
                }
                else
                {
                    childIndex = treeView.viewController.GetChildIndexForId(rootID) + 1;
                    rootID = treeView.viewController.GetParentId(rootID);
                    Debug.Log($"InsertNewTree {rootID},{childIndex}");
                }
            }
            
            var newId = treeView.viewController.GetAllItemIds().DefaultIfEmpty(-1).Max() + 1;
            SerializableTree<ITEM>.TreeNode node = new()
            {
                node = new SerializableTree<ITEM>.Node()
                {
                    Name = name,
                    Parent = -1,
                    Item = nodeItem
                },
                children = new(),
                index = newId
            };
            var item = new TreeViewItemData<SerializableTree<ITEM>.TreeNode>(newId,node)
            {
                
            };
            treeView.AddItem(item,rootID,childIndex);
            OnHerarchyChanged();
        }

        private void RemoveTree(int rootID)
        {
            if (treeView.TryRemoveItem(rootID, false))
            {
                OnHerarchyChanged();
            }
        }

        public void TreeNameChange( int id , string newName )
        {
            var node = treeView.viewController.GetItemForId(id) as SerializableTree<ITEM>.TreeNode;
            node.node.Name = newName;
            OnHerarchyChanged();
        }

        private void OnHerarchyChanged()
        {
            if (treeView.viewController != null)
            {
                var treeNode = TraverseTree(treeView.viewController.GetRootItemIds());
                hierarchyChanged?.Invoke(treeNode);
            }
        }

        private List<SerializableTree<ITEM>.TreeNode> TraverseTree(IEnumerable<int> root, int parentIdx = -1)
        {
            List<SerializableTree<ITEM>.TreeNode> tree = new();

            foreach (var node in root)
            {
                var treeNode = new SerializableTree<ITEM>.TreeNode();
                var data = treeView.viewController.GetItemForId(node) as SerializableTree<ITEM>.TreeNode;
                var childrenIds = treeView.viewController.GetChildrenIds(node);
                treeNode.node.Name = data.node.Name;
                treeNode.node.Item = data.node.Item;
                treeNode.node.Parent = parentIdx;
                treeNode.children = TraverseTree(childrenIds, data.index);
                tree.Add(treeNode);
            }

            return tree;
        }

        private bool CheckTree(SerializableTree<ITEM> tree)
        {
            var root = treeView.viewController.GetRootItemIds();

            if (root.Count() != tree.Nodes.Length)
            {
                return false;
            }
            
            for (int i = 0; i < tree.Nodes.Length; i++)
            {
                if (tree.Nodes[i].Parent != treeView.viewController.GetParentId(i))
                {
                    return false;
                }
            }

            return true;
        }

        private void OnSearchBarValueChangedCallback( ChangeEvent<string> value)
        {
            var newValue = value.newValue.ToLower();
            for( int i = 0; i < treeView.GetTreeCount(); i++ )
            {
                var item = treeView.GetItemDataForIndex<SerializableTree<ITEM>.TreeNode>(i);
                var name = item.node.Name.ToLower();

                if (String.IsNullOrEmpty(newValue))
                {
                    var element = treeView.GetRootElementForIndex(i);                    
                    element.style.display = DisplayStyle.Flex;
                }
                else if (!name.Contains(newValue) )
                {
                    var element = treeView.GetRootElementForIndex(i);                    
                    element.style.display = DisplayStyle.None;
                }
                else
                {
                    void SetDisp( int index )
                    {
                        var element = treeView.GetRootElementForId(index);                        
                        element.style.display = DisplayStyle.Flex;
                        var parentID = treeView.GetParentIdForIndex(index);
                        if (parentID >= 0)
                        {
                            var parentIndex = treeView.viewController.GetIndexForId(parentID);
                            SetDisp(parentIndex);
                        }
                    }
                    SetDisp(i);
                }
            }
        }

        private List<TreeViewItemData<SerializableTree<ITEM>.TreeNode>> MakeTree(List<SerializableTree<ITEM>.TreeNode> tree)
        {
            var root = tree
                .Select(t => new TreeViewItemData<SerializableTree<ITEM>.TreeNode>(t.index, t, MakeTree(t.children)) { })
                .ToList();
            return root;
        }
    }
}