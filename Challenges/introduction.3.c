typedef struct Node
{
    struct Node *next;
    void* value;
} Node;

int main(int argc, char** argv)
{
    int v = 5;

    Node n0 = { .value = &v };
    Node n1 = { .value = n0.value };
    n0.next = &n1;
}
