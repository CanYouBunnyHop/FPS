public class Ref<T>
{
    private T backing;
    public T Value {get{return backing;}}
    public Ref(T reference)
    {
        backing = reference;
    }
}
